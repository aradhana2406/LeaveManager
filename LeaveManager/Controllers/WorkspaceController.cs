using LeaveManager.Common.Enums;
using LeaveManager.Data;
using LeaveManager.Entities;
using LeaveManager.Features.EmployeeManagement.Services;
using LeaveManager.Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Controllers;

[ApiController]
[Route("api/workspace")]
public partial class WorkspaceController : ControllerBase
{
    private const string SickCasualLeaveName = "Sick/Casual Leave";
    private const string PlannedLeaveName = "Planned Leave";
    private const string MaternityLeaveName = "Maternity Leave";
    private const string DirectorSpecialLeaveName = "Director Special Leave";
    private const string DefaultTemporaryPassword = "welcome123";
    private readonly AppDbContext _context;
    private readonly ILeaveAccrualService _leaveAccrualService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkspaceController> _logger;

    public WorkspaceController(
        AppDbContext context,
        ILeaveAccrualService leaveAccrualService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<WorkspaceController> logger)
    {
        _context = context;
        _leaveAccrualService = leaveAccrualService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkspace(CancellationToken cancellationToken)
    {
        await _leaveAccrualService.SyncAllAsync(cancellationToken);

        var employees = await _context.Employees
            .AsNoTracking()
            .Include(x => x.OrganizationRole)
            .Include(x => x.PrimaryTeam)
                .ThenInclude(x => x!.Lead)
            .Include(x => x.EmployeeLeaveBalances)
                .ThenInclude(x => x.LeaveType)
            .Include(x => x.EmployeeTeams)
                .ThenInclude(x => x.Team)
                    .ThenInclude(x => x.Project)
            .Include(x => x.EmployeeTeams)
                .ThenInclude(x => x.Team)
                    .ThenInclude(x => x.Lead)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        var projects = await _context.Projects
            .AsNoTracking()
            .Include(x => x.Teams)
                .ThenInclude(x => x.Lead)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var hrPolicy = await GetOrCreateHrPolicyAsync(cancellationToken);
        return Ok(new
        {
            Employees = employees.Select(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.FullName,
                x.Email,
                x.Department,
                x.Designation,
                x.JobRole,
                x.EmploymentType,
                x.Location,
                x.SalaryStructureDetails,
                x.JoinDate,
                Role = x.Role.ToString(),
                RoleLabel = x.Role.GetDisplayName(),
                RoleId = (int)x.Role,
                RoleSelection = ((int)x.Role).ToString(),
                x.PrimaryTeamId,
                PrimaryTeam = x.PrimaryTeam == null
                    ? null
                    : new
                    {
                        x.PrimaryTeam.Id,
                        x.PrimaryTeam.Name,
                        LeadName = x.PrimaryTeam.Lead.FullName
                    },
                LeaveBalances = x.EmployeeLeaveBalances
                    .OrderBy(lb => lb.LeaveType.Name)
                    .Select(lb => new
                    {
                        lb.LeaveTypeId,
                        LeaveTypeName = lb.LeaveType.Name,
                        AllocatedLeaves = lb.AllocatedLeaves,
                        UsedLeaves = lb.UsedLeaves,
                        RemainingLeaves = lb.AllocatedLeaves - lb.UsedLeaves
                    })
                    .ToList(),
                Teams = x.EmployeeTeams
                    .Select(et => new
                    {
                        et.Team.Id,
                        et.Team.Name,
                        ProjectName = et.Team.Project.Name,
                        LeadName = et.Team.Lead.FullName
                    })
                    .ToList()
            }),
            Projects = projects.Select(x => new
            {
                x.Id,
                x.Name,
                Teams = x.Teams
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.ProjectId,
                        t.LeadId,
                        LeadName = t.Lead.FullName
                    })
                    .ToList()
            }),
            LeaveTypes = await _context.LeaveTypes
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.IsPaid,
                    x.RequiresAdvanceNotice,
                    x.AdvanceNoticeDays
                })
                .ToListAsync(cancellationToken),
            Roles = Enum.GetValues<Role>()
                .Select(x => new
                {
                    Id = ((int)x).ToString(),
                    Name = x.ToString(),
                    Label = x.GetDisplayName(),
                    BaseRoleId = (int)x,
                    IsCustom = false
                })
                .ToList(),
            HrPolicy = new
            {
                hrPolicy.AllowHalfDayLeave
            }
        });
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeCode) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Department) ||
            string.IsNullOrWhiteSpace(request.Designation) ||
            string.IsNullOrWhiteSpace(request.JobRole) ||
            string.IsNullOrWhiteSpace(request.EmploymentType) ||
            string.IsNullOrWhiteSpace(request.Location) ||
            string.IsNullOrWhiteSpace(request.SalaryStructureDetails))
        {
            return BadRequest(new { Message = "Employee code, full name, official email, department, designation, role, joining date, employment type, location, and salary details are required." });
        }

        var roleSelection = await ResolveRoleSelectionAsync(request.RoleSelection, request.Role, cancellationToken);
        if (roleSelection == null)
        {
            return BadRequest(new { Message = "Invalid role selected." });
        }

        if (request.JoinDate == default)
        {
            return BadRequest(new { Message = "Join date is required." });
        }

        var employeeCode = request.EmployeeCode.Trim();
        var email = request.Email.Trim();

        var duplicateExists = await _context.Employees.AnyAsync(
            x => x.EmployeeCode == employeeCode || x.Email == email,
            cancellationToken);

        if (duplicateExists)
        {
            return BadRequest(new { Message = "Employee code or email already exists." });
        }

        var teamIds = request.TeamIds
            .Append(request.PrimaryTeamId)
            .Distinct()
            .ToList();

        var teams = await _context.Teams
            .Where(x => teamIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (teams.Count != teamIds.Count)
        {
            return BadRequest(new { Message = "One or more selected teams were not found." });
        }

        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            FullName = request.FullName.Trim(),
            Email = email,
            Department = request.Department.Trim(),
            Designation = request.Designation.Trim(),
            JobRole = request.JobRole.Trim(),
            EmploymentType = request.EmploymentType.Trim(),
            Location = request.Location.Trim(),
            SalaryStructureDetails = request.SalaryStructureDetails.Trim(),
            JoinDate = DateTime.SpecifyKind(request.JoinDate.Date, DateTimeKind.Utc),
            Role = roleSelection.Value.Role,
            OrganizationRoleId = roleSelection.Value.OrganizationRoleId,
            PrimaryTeamId = request.PrimaryTeamId,
            IsActive = true
        };

        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var memberships = teamIds.Select(teamId => new EmployeeTeam
        {
            EmployeeId = employee.Id,
            TeamId = teamId
        });

        await _context.EmployeeTeams.AddRangeAsync(memberships, cancellationToken);
        await CreateDefaultLeaveBalancesAsync(employee.Id, cancellationToken);
        var login = await CreateDefaultLoginAsync(employee, cancellationToken);
        await _leaveAccrualService.SyncEmployeeAsync(employee.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var invitationEmailSent = await TrySendOnboardingInviteAsync(employee, login.Username);

        return Ok(new
        {
            Message = invitationEmailSent
                ? "Employee registered successfully and the onboarding invite email was sent."
                : "Employee registered successfully. Invite email could not be sent, so share the temporary login manually.",
            EmployeeId = employee.Id,
            LoginUsername = login.Username,
            TemporaryPassword = login.Password,
            InvitationEmailSent = invitationEmailSent
        });
    }

    [HttpPut("employees/{employeeId:int}")]
    public async Task<IActionResult> UpdateEmployee(
        int employeeId,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeCode) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Department) ||
            string.IsNullOrWhiteSpace(request.Designation) ||
            string.IsNullOrWhiteSpace(request.JobRole) ||
            string.IsNullOrWhiteSpace(request.EmploymentType) ||
            string.IsNullOrWhiteSpace(request.Location) ||
            string.IsNullOrWhiteSpace(request.SalaryStructureDetails))
        {
            return BadRequest(new { Message = "Employee code, full name, official email, department, designation, role, joining date, employment type, location, and salary details are required." });
        }

        var roleSelection = await ResolveRoleSelectionAsync(request.RoleSelection, request.Role, cancellationToken);
        if (roleSelection == null)
        {
            return BadRequest(new { Message = "Invalid role selected." });
        }

        if (request.JoinDate == default)
        {
            return BadRequest(new { Message = "Join date is required." });
        }

        var employee = await _context.Employees
            .Include(x => x.EmployeeTeams)
            .FirstOrDefaultAsync(x => x.Id == employeeId && x.IsActive, cancellationToken);

        if (employee == null)
        {
            return NotFound(new { Message = "Employee was not found." });
        }

        var employeeCode = request.EmployeeCode.Trim();
        var email = request.Email.Trim();

        var duplicateExists = await _context.Employees.AnyAsync(
            x => x.Id != employeeId && (x.EmployeeCode == employeeCode || x.Email == email),
            cancellationToken);

        if (duplicateExists)
        {
            return BadRequest(new { Message = "Employee code or email already exists." });
        }

        var teamIds = request.TeamIds
            .Append(request.PrimaryTeamId)
            .Distinct()
            .ToList();

        var teams = await _context.Teams
            .Where(x => teamIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (teams.Count != teamIds.Count)
        {
            return BadRequest(new { Message = "One or more selected teams were not found." });
        }

        employee.EmployeeCode = employeeCode;
        employee.FullName = request.FullName.Trim();
        employee.Email = email;
        employee.Department = request.Department.Trim();
        employee.Designation = request.Designation.Trim();
        employee.JobRole = request.JobRole.Trim();
        employee.EmploymentType = request.EmploymentType.Trim();
        employee.Location = request.Location.Trim();
        employee.SalaryStructureDetails = request.SalaryStructureDetails.Trim();
        employee.JoinDate = DateTime.SpecifyKind(request.JoinDate.Date, DateTimeKind.Utc);
        employee.Role = roleSelection.Value.Role;
        employee.OrganizationRoleId = roleSelection.Value.OrganizationRoleId;
        employee.PrimaryTeamId = request.PrimaryTeamId;

        _context.EmployeeTeams.RemoveRange(employee.EmployeeTeams);

        var memberships = teamIds.Select(teamId => new EmployeeTeam
        {
            EmployeeId = employee.Id,
            TeamId = teamId
        });

        await _context.EmployeeTeams.AddRangeAsync(memberships, cancellationToken);

        var login = await _context.UserLogins
            .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id, cancellationToken);

        if (login != null)
        {
            login.Username = employeeCode.ToLowerInvariant();
        }

        await _leaveAccrualService.SyncEmployeeAsync(employee.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Employee updated successfully.", EmployeeId = employee.Id });
    }

    [HttpPut("hr-policy")]
    public async Task<IActionResult> UpdateHrPolicy(
        UpdateHrPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await GetOrCreateHrPolicyAsync(cancellationToken);
        policy.AllowHalfDayLeave = request.AllowHalfDayLeave;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "HR policy updated successfully." });
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateOrganizationRole(
        CreateOrganizationRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Label))
        {
            return BadRequest(new { Message = "Role name is required." });
        }

        if (!Enum.IsDefined(typeof(Role), request.BaseRole))
        {
            return BadRequest(new { Message = "Select a valid permission band for this role." });
        }

        var label = request.Label.Trim();
        var name = NormalizeRoleName(label);

        var duplicateExists = await _context.OrganizationRoles.AnyAsync(
            x => x.Name == name || x.Label == label,
            cancellationToken);

        if (duplicateExists)
        {
            return BadRequest(new { Message = "A role with this name already exists." });
        }

        var role = new OrganizationRole
        {
            Name = name,
            Label = label,
            BaseRole = request.BaseRole,
            IsActive = true
        };

        await _context.OrganizationRoles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Role added successfully.", RoleId = role.Id });
    }

    [HttpPost("projects")]
    public async Task<IActionResult> CreateProject(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Project name is required." });
        }

        var project = new Project { Name = request.Name.Trim() };
        await _context.Projects.AddAsync(project, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Project created successfully.", ProjectId = project.Id });
    }

    [HttpPut("projects/{projectId:int}")]
    public async Task<IActionResult> UpdateProject(
        int projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Project name is required." });
        }

        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);

        if (project == null)
        {
            return NotFound(new { Message = "Project was not found." });
        }

        project.Name = request.Name.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Project updated successfully.", ProjectId = project.Id });
    }

    [HttpPost("teams")]
    public async Task<IActionResult> CreateTeam(
        CreateTeamRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Team name is required." });
        }

        var projectExists = await _context.Projects.AnyAsync(
            x => x.Id == request.ProjectId,
            cancellationToken);

        if (!projectExists)
        {
            return BadRequest(new { Message = "Selected project was not found." });
        }

        var leadExists = await _context.Employees.AnyAsync(
            x => x.Id == request.LeadId && x.IsActive,
            cancellationToken);

        if (!leadExists)
        {
            return BadRequest(new { Message = "Selected team lead was not found." });
        }

        var team = new Team
        {
            Name = request.Name.Trim(),
            ProjectId = request.ProjectId,
            LeadId = request.LeadId
        };

        await _context.Teams.AddAsync(team, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Team created successfully.", TeamId = team.Id });
    }

    [HttpPut("teams/{teamId:int}")]
    public async Task<IActionResult> UpdateTeam(
        int teamId,
        UpdateTeamRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Team name is required." });
        }

        var team = await _context.Teams
            .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);

        if (team == null)
        {
            return NotFound(new { Message = "Team was not found." });
        }

        var projectExists = await _context.Projects.AnyAsync(
            x => x.Id == request.ProjectId,
            cancellationToken);

        if (!projectExists)
        {
            return BadRequest(new { Message = "Selected project was not found." });
        }

        var leadExists = await _context.Employees.AnyAsync(
            x => x.Id == request.LeadId && x.IsActive,
            cancellationToken);

        if (!leadExists)
        {
            return BadRequest(new { Message = "Selected team lead was not found." });
        }

        team.Name = request.Name.Trim();
        team.ProjectId = request.ProjectId;
        team.LeadId = request.LeadId;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Team updated successfully.", TeamId = team.Id });
    }
}

public class CreateEmployeeRequest
{
    public string EmployeeCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public string JobRole { get; set; } = string.Empty;

    public string EmploymentType { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string SalaryStructureDetails { get; set; } = string.Empty;

    public DateTime JoinDate { get; set; }

    public Role Role { get; set; }

    public string? RoleSelection { get; set; }

    public int PrimaryTeamId { get; set; }

    public List<int> TeamIds { get; set; } = new();
}

public class UpdateEmployeeRequest
{
    public string EmployeeCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public string JobRole { get; set; } = string.Empty;

    public string EmploymentType { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string SalaryStructureDetails { get; set; } = string.Empty;

    public DateTime JoinDate { get; set; }

    public Role Role { get; set; }

    public string? RoleSelection { get; set; }

    public int PrimaryTeamId { get; set; }

    public List<int> TeamIds { get; set; } = new();
}

public class UpdateHrPolicyRequest
{
    public bool AllowHalfDayLeave { get; set; }
}

public class CreateOrganizationRoleRequest
{
    public string Label { get; set; } = string.Empty;

    public Role BaseRole { get; set; } = Role.Employee;
}

public partial class WorkspaceController
{
    private async Task<HrPolicy> GetOrCreateHrPolicyAsync(CancellationToken cancellationToken)
    {
        var policy = await _context.HrPolicies.FirstOrDefaultAsync(cancellationToken);
        if (policy != null)
        {
            return policy;
        }

        policy = new HrPolicy { AllowHalfDayLeave = false };
        await _context.HrPolicies.AddAsync(policy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return policy;
    }

    private async Task<(Role Role, int? OrganizationRoleId)?> ResolveRoleSelectionAsync(
        string? roleSelection,
        Role fallbackRole,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(roleSelection))
        {
            var selection = roleSelection.Trim();

            if (selection.StartsWith("custom-", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(selection["custom-".Length..], out var organizationRoleId))
            {
                var customRole = await _context.OrganizationRoles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == organizationRoleId && x.IsActive, cancellationToken);

                return customRole == null
                    ? null
                    : (customRole.BaseRole, customRole.Id);
            }

            if (int.TryParse(selection, out var roleId) && Enum.IsDefined(typeof(Role), roleId))
            {
                return ((Role)roleId, null);
            }
        }

        return Enum.IsDefined(typeof(Role), fallbackRole)
            ? (fallbackRole, null)
            : null;
    }

    private static string NormalizeRoleName(string label)
    {
        var lettersAndDigits = label
            .Where(char.IsLetterOrDigit)
            .ToArray();

        return lettersAndDigits.Length == 0
            ? Guid.NewGuid().ToString("N")
            : new string(lettersAndDigits);
    }

    private async Task CreateDefaultLeaveBalancesAsync(int employeeId, CancellationToken cancellationToken)
    {
        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var defaultBalances = new Dictionary<string, decimal>
        {
            [SickCasualLeaveName] = 7m,
            [PlannedLeaveName] = 1m,
            [MaternityLeaveName] = 60m,
            [DirectorSpecialLeaveName] = 0m
        };

        var balances = leaveTypes
            .Where(leaveType => defaultBalances.ContainsKey(leaveType.Name))
            .Select(leaveType => new EmployeeLeaveBalance
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveType.Id,
                AllocatedLeaves = defaultBalances[leaveType.Name],
                UsedLeaves = 0m
            })
            .ToList();

        if (balances.Count > 0)
        {
            await _context.EmployeeLeaveBalances.AddRangeAsync(balances, cancellationToken);
        }
    }

    private async Task<UserLogin> CreateDefaultLoginAsync(Employee employee, CancellationToken cancellationToken)
    {
        var username = employee.EmployeeCode.Trim().ToLowerInvariant();
        var login = await _context.UserLogins
            .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id, cancellationToken);

        if (login == null)
        {
            login = new UserLogin
            {
                EmployeeId = employee.Id
            };

            await _context.UserLogins.AddAsync(login, cancellationToken);
        }

        login.Username = username;
        login.Password = DefaultTemporaryPassword;
        login.IsActive = true;

        return login;
    }

    private async Task<bool> TrySendOnboardingInviteAsync(Employee employee, string username)
    {
        try
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            await _emailService.SendEmailAsync(
                employee.Email,
                "Complete your onboarding details - LeaveManager",
                GenerateOnboardingInviteEmail(employee, username, baseUrl));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send onboarding invite to employee {EmployeeId}", employee.Id);
            return false;
        }
    }

    private static string GenerateOnboardingInviteEmail(Employee employee, string username, string baseUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; color: #24302b;'>
    <div style='max-width: 680px; margin: 0 auto; padding: 24px;'>
        <h2 style='margin-bottom: 12px;'>Welcome to the company, {employee.FullName}</h2>
        <p>HR has completed the company-side onboarding for your employee profile. Your next step is to log in and complete part 2 of onboarding.</p>
        <div style='padding: 16px; border: 1px solid #ded7c8; border-radius: 8px; background: #fffdf8; margin: 18px 0;'>
            <p style='margin: 0 0 8px;'><strong>Employee Code:</strong> {employee.EmployeeCode}</p>
            <p style='margin: 0 0 8px;'><strong>Username:</strong> {username}</p>
            <p style='margin: 0;'><strong>Temporary password:</strong> {DefaultTemporaryPassword}</p>
        </div>
        <p>Please log in to LeaveManager and complete your personal onboarding details, including identity and prior-experience documents.</p>
        <p><a href='{baseUrl}' style='display: inline-block; padding: 12px 18px; background: #315f4f; color: white; text-decoration: none; border-radius: 6px;'>Open LeaveManager</a></p>
        <p style='margin-top: 20px; color: #66736e;'>If you cannot access the portal, contact HR.</p>
    </div>
</body>
</html>";
    }
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public int LeadId { get; set; }
}

public class UpdateTeamRequest
{
    public string Name { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public int LeadId { get; set; }
}
