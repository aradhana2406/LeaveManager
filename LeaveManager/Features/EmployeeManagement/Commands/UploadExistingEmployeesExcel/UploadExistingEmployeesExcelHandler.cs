using LeaveManager.Common.Enums;
using LeaveManager.Data;
using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Features.EmployeeManagement.Commands.UploadExistingEmployeesExcel;

public class UploadExistingEmployeesExcelHandler
{
    private const string DefaultPassword = "welcome123";
    private readonly AppDbContext _context;
    private readonly IExistingEmployeeExcelParser _parser;

    public UploadExistingEmployeesExcelHandler(
        AppDbContext context,
        IExistingEmployeeExcelParser parser)
    {
        _context = context;
        _parser = parser;
    }

    public async Task<UploadExistingEmployeesExcelResponse> Handle(
        UploadExistingEmployeesExcelCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rows = await _parser.ParseAsync(request.File);
            if (!rows.Any())
            {
                return new UploadExistingEmployeesExcelResponse
                {
                    Success = false,
                    Message = "No employee rows found in Excel file."
                };
            }

            var processed = 0;
            var failed = 0;
            var skipped = 0;
            var errors = new List<string>();
            var skippedRows = new List<string>();

            foreach (var row in rows)
            {
                var rowResult = await ImportRowAsync(row, cancellationToken);
                if (rowResult.Skipped)
                {
                    skipped++;
                    skippedRows.Add(rowResult.Message);
                    continue;
                }

                if (rowResult.Errors.Count > 0)
                {
                    failed++;
                    errors.AddRange(rowResult.Errors);
                    continue;
                }

                processed++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new UploadExistingEmployeesExcelResponse
            {
                Success = failed == 0,
                Message = $"Inserted: {processed}, Skipped: {skipped}, Failed: {failed}",
                RecordsProcessed = processed,
                RecordsFailed = failed,
                RecordsSkipped = skipped,
                Errors = skippedRows.Concat(errors).ToList()
            };
        }
        catch (Exception ex)
        {
            return new UploadExistingEmployeesExcelResponse
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}",
                Errors = new() { ex.Message }
            };
        }
    }

    private async Task<ImportExistingEmployeeRowResult> ImportRowAsync(
        ExistingEmployeeExcelRow row,
        CancellationToken cancellationToken)
    {
        var errors = ValidateRequiredFields(row);
        if (errors.Count > 0)
        {
            return ImportExistingEmployeeRowResult.Failed(errors);
        }

        var employeeCode = row.EmployeeCode.Trim();
        var existingEmployee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode, cancellationToken);

        if (existingEmployee != null)
        {
            return ImportExistingEmployeeRowResult.Skip(
                $"Row {row.RowNumber}: Employee code '{employeeCode}' already exists, skipped without changes.");
        }

        if (!TryParseRole(row.Role, out var role))
        {
            errors.Add($"Row {row.RowNumber}: SystemAccess '{row.Role}' is invalid.");
        }

        if (row.JoinDate == default)
        {
            errors.Add($"Row {row.RowNumber}: JoinDate is required or invalid.");
        }

        if (await _context.Employees.AnyAsync(x => x.Email == row.OfficialEmail.Trim(), cancellationToken))
        {
            errors.Add($"Row {row.RowNumber}: Email '{row.OfficialEmail}' already exists for another employee.");
        }

        if (GetTeamNames(row).Count > 0 && string.IsNullOrWhiteSpace(row.ProjectName))
        {
            errors.Add($"Row {row.RowNumber}: ProjectName is required when team names are provided.");
        }

        var username = string.IsNullOrWhiteSpace(row.Username)
            ? row.EmployeeCode.Trim().ToLowerInvariant()
            : row.Username.Trim();

        if (await _context.UserLogins.AnyAsync(x => x.Username == username, cancellationToken))
        {
            errors.Add($"Row {row.RowNumber}: Username '{username}' already exists.");
        }

        if (errors.Count > 0)
        {
            return ImportExistingEmployeeRowResult.Failed(errors);
        }

        var project = await GetOrCreateProjectAsync(row.ProjectName, cancellationToken);
        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            FullName = row.FullName.Trim(),
            Email = row.OfficialEmail.Trim(),
            Department = row.Department.Trim(),
            Designation = row.Designation.Trim(),
            JobRole = row.JobRole.Trim(),
            EmploymentType = row.EmploymentType.Trim(),
            Location = row.Location.Trim(),
            SalaryStructureDetails = row.SalaryStructureDetails.Trim(),
            JoinDate = DateTime.SpecifyKind(row.JoinDate.Date, DateTimeKind.Utc),
            Role = role,
            IsActive = true
        };

        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var teamNames = GetTeamNames(row);
        var primaryTeamId = 0;
        foreach (var teamName in teamNames)
        {
            var team = await GetOrCreateTeamAsync(project!, teamName, row.TeamLeadEmpcode, employee.Id, cancellationToken);
            await _context.EmployeeTeams.AddAsync(new EmployeeTeam
            {
                EmployeeId = employee.Id,
                TeamId = team.Id
            }, cancellationToken);

            if (string.Equals(teamName, row.PrimaryTeam.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                primaryTeamId = team.Id;
            }
        }

        if (primaryTeamId > 0)
        {
            employee.PrimaryTeamId = primaryTeamId;
        }

        await _context.UserLogins.AddAsync(new UserLogin
        {
            EmployeeId = employee.Id,
            Username = username,
            Password = string.IsNullOrWhiteSpace(row.TemporaryPassword)
                ? DefaultPassword
                : row.TemporaryPassword.Trim(),
            IsActive = true
        }, cancellationToken);

        await CreateDefaultLeaveBalancesAsync(employee.Id, cancellationToken);
        return ImportExistingEmployeeRowResult.Inserted();
    }

    private static List<string> ValidateRequiredFields(ExistingEmployeeExcelRow row)
    {
        var errors = new List<string>();
        var required = new Dictionary<string, string>
        {
            ["EmployeeCode"] = row.EmployeeCode,
            ["FullName"] = row.FullName,
            ["OfficialEmail"] = row.OfficialEmail,
            ["Department"] = row.Department,
            ["Designation"] = row.Designation,
            ["Role"] = row.JobRole,
            ["EmploymentType"] = row.EmploymentType,
            ["Location"] = row.Location,
            ["SystemAccess"] = row.Role,
            ["SalaryStructureDetails"] = row.SalaryStructureDetails
        };

        foreach (var item in required)
        {
            if (string.IsNullOrWhiteSpace(item.Value))
            {
                errors.Add($"Row {row.RowNumber}: {item.Key} is required.");
            }
        }

        return errors;
    }

    private async Task<Project?> GetOrCreateProjectAsync(
        string projectName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            return null;
        }

        var normalized = projectName.Trim();
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Name == normalized, cancellationToken);

        if (project != null)
        {
            return project;
        }

        project = new Project { Name = normalized };
        await _context.Projects.AddAsync(project, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return project;
    }

    private async Task<Team> GetOrCreateTeamAsync(
        Project project,
        string teamName,
        string leadEmpcode,
        int fallbackLeadId,
        CancellationToken cancellationToken)
    {
        var normalized = teamName.Trim();
        var team = await _context.Teams
            .FirstOrDefaultAsync(
                x => x.ProjectId == project.Id && x.Name == normalized,
                cancellationToken);

        if (team != null)
        {
            return team;
        }

        var lead = !string.IsNullOrWhiteSpace(leadEmpcode)
            ? await _context.Employees.FirstOrDefaultAsync(
                x => x.EmployeeCode == leadEmpcode.Trim() && x.IsActive,
                cancellationToken)
            : null;

        team = new Team
        {
            Name = normalized,
            ProjectId = project.Id,
            LeadId = lead?.Id ?? fallbackLeadId
        };

        await _context.Teams.AddAsync(team, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return team;
    }

    private async Task CreateDefaultLeaveBalancesAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var defaultBalances = new Dictionary<string, decimal>
        {
            ["Sick/Casual Leave"] = 7m,
            ["Planned Leave"] = 1m,
            ["Maternity Leave"] = 60m,
            ["Director Special Leave"] = 0m
        };

        var balances = leaveTypes
            .Where(leaveType => defaultBalances.ContainsKey(leaveType.Name))
            .Select(leaveType => new EmployeeLeaveBalance
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveType.Id,
                AllocatedLeaves = defaultBalances[leaveType.Name],
                UsedLeaves = 0m
            });

        await _context.EmployeeLeaveBalances.AddRangeAsync(balances, cancellationToken);
    }

    private static List<string> GetTeamNames(ExistingEmployeeExcelRow row)
    {
        return new[] { row.PrimaryTeam }
            .Concat((row.AdditionalTeams ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(team => team.Trim())
            .Where(team => !string.IsNullOrWhiteSpace(team))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool TryParseRole(string value, out Role role)
    {
        var normalized = (value ?? string.Empty)
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();

        var map = new Dictionary<string, Role>
        {
            ["employee"] = Role.Employee,
            ["teamlead"] = Role.TeamLead,
            ["hr"] = Role.HR,
            ["hrl1"] = Role.HR,
            ["seniorsoftwareengineer"] = Role.SeniorSoftwareEngineer,
            ["manager"] = Role.Manager,
            ["managerl1"] = Role.Manager,
            ["technicalmanagerl1"] = Role.Manager,
            ["organizationhead"] = Role.OrganizationHead,
            ["orghead"] = Role.OrganizationHead,
            ["hrl2"] = Role.HRL2,
            ["managerl2"] = Role.ManagerL2,
            ["technicalmanagerl2"] = Role.ManagerL2,
            ["softwareengineer"] = Role.Employee
        };

        return map.TryGetValue(normalized, out role);
    }

    private class ImportExistingEmployeeRowResult
    {
        public bool Skipped { get; private init; }

        public string Message { get; private init; } = string.Empty;

        public List<string> Errors { get; private init; } = new();

        public static ImportExistingEmployeeRowResult Inserted()
        {
            return new ImportExistingEmployeeRowResult();
        }

        public static ImportExistingEmployeeRowResult Skip(string message)
        {
            return new ImportExistingEmployeeRowResult
            {
                Skipped = true,
                Message = message
            };
        }

        public static ImportExistingEmployeeRowResult Failed(List<string> errors)
        {
            return new ImportExistingEmployeeRowResult
            {
                Errors = errors
            };
        }
    }
}
