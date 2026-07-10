using LeaveManager.Common.Enums;
using LeaveManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Message = "Username and password are required." });
        }

        var login = await _context.UserLogins
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(x => x.OrganizationRole)
            .FirstOrDefaultAsync(
                x => x.Username == request.Username.Trim() &&
                     x.Password == request.Password &&
                     x.IsActive &&
                     x.Employee.IsActive,
                cancellationToken);

        if (login == null)
        {
            return Unauthorized(new { Message = "Invalid username or password." });
        }

        var role = login.Employee.Role;
        var canApprove = role.IsApproverRole();

        return Ok(new
        {
            Username = login.Username,
            EmployeeId = login.EmployeeId,
            login.Employee.FullName,
            Role = role.ToString(),
            RoleLabel = role.GetDisplayName(),
            Views = GetViews(role, canApprove)
        });
    }

    [HttpGet("demo-users")]
    public async Task<IActionResult> GetDemoUsers(CancellationToken cancellationToken)
    {
        var users = await _context.UserLogins
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => x.IsActive && x.Employee.IsActive)
            .OrderBy(x => x.Username)
            .Select(x => new
            {
                x.Username,
                x.Employee.FullName,
                Role = x.Employee.Role.GetDisplayName(),
                Hint = "demo123"
            })
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    private static List<string> GetViews(Role role, bool canApprove)
    {
        var views = new List<string>();

        if (role.IsHrRole())
        {
            views.AddRange(new[] { "directory", "hrHome", "hrControl", "register", "review", "deviceManagement", "projects", "balances", "apply", "onboarding" });
            return views.Distinct().ToList();
        }

        if (role == Role.OrganizationHead)
        {
            views.AddRange(new[] { "overview", "review", "directory", "apply", "onboarding", "deviceManagement" });
            return views.Distinct().ToList();
        }

        if (role == Role.TeamLead)
        {
            views.AddRange(new[] { "directory", "review", "apply", "onboarding", "deviceManagement" });
            return views.Distinct().ToList();
        }

        if (canApprove)
        {
            views.Add("review");
        }

        views.AddRange(new[] { "directory", "apply", "onboarding", "deviceManagement" });
        return views.Distinct().ToList();
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
