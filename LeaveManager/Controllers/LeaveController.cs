using LeaveManager.Common.Enums;
using LeaveManager.Data;
using LeaveManager.Features.EmployeeManagement.Commands.ApplyLeave;
using LeaveManager.Features.Leave.Commands.ApproveRejectLeave;
using LeaveManager.Infrastructure.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LeaveManager.Controllers;

[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly ApplyLeaveHandler _applyLeaveHandler;
    private readonly ApproveLeaveHandler _approveLeaveHandler;
    private readonly IApprovalTokenService _tokenService;
    private readonly ILogger<LeaveController> _logger;
    private readonly AppDbContext _context;

    public LeaveController(
        ApplyLeaveHandler applyLeaveHandler,
        ApproveLeaveHandler approveLeaveHandler,
        IApprovalTokenService tokenService,
        ILogger<LeaveController> logger,
        AppDbContext context)
    {
        _applyLeaveHandler = applyLeaveHandler;
        _approveLeaveHandler = approveLeaveHandler;
        _tokenService = tokenService;
        _logger = logger;
        _context = context;
    }

    [HttpPost("apply-leave")]
    [ProducesResponseType(typeof(ApplyLeaveResponse), 200)]
    [ProducesResponseType(typeof(ApplyLeaveResponse), 400)]
    public async Task<IActionResult> ApplyLeave(
        [FromBody] ApplyLeaveCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _applyLeaveHandler.Handle(command, cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("employee/{employeeId:int}/requests")]
    public async Task<IActionResult> GetEmployeeRequests(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var employeeExists = await _context.Employees
            .AsNoTracking()
            .AnyAsync(x => x.Id == employeeId && x.IsActive, cancellationToken);

        if (!employeeExists)
        {
            return NotFound(new { Message = "Employee not found." });
        }

        var requests = await _context.LeaveApplications
            .AsNoTracking()
            .Include(x => x.LeaveType)
            .Include(x => x.Approver)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.AppliedOn ?? DateTime.UtcNow)
            .ThenByDescending(x => x.FromDate)
            .Select(x => new
            {
                x.Id,
                x.LeaveTypeId,
                LeaveTypeName = x.LeaveType.Name,
                x.FromDate,
                x.ToDate,
                x.Reason,
                x.ApprovalReason,
                x.AppliedOn,
                x.ApprovedOn,
                x.RejectedOn,
                ActionedOn = x.Status == "Cancelled"
                    ? x.RejectedOn
                    : x.ApprovedOn ?? x.RejectedOn,
                x.Status,
                ApproverName = x.Approver != null ? x.Approver.FullName : null,
                TotalDays = x.TotalDays > 0
                    ? x.TotalDays
                    : EF.Functions.DateDiffDay(x.FromDate, x.ToDate) + 1,
                CanCancel = x.Status == "Pending" || x.Status == "Approved"
            })
            .ToListAsync(cancellationToken);

        return Ok(new { Requests = requests });
    }

    [HttpGet("approve")]
    [ProducesResponseType(typeof(ApproveLeaveResponse), 200)]
    [ProducesResponseType(typeof(ApproveLeaveResponse), 400)]
    public async Task<IActionResult> ApproveLeaveFromEmail(
        [FromQuery] string token,
        [FromQuery] string action,
        [FromQuery] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (leaveAppId, approverId) = _tokenService.DecodeToken(token);

            var command = new ApproveLeaveCommand
            {
                LeaveApplicationId = leaveAppId,
                ApproverId = approverId,
                Action = action,
                Reason = reason
            };

            var result = await _approveLeaveHandler.Handle(command, cancellationToken);

            if (result.Success)
            {
                return Content(
                    BuildLeaveDecisionPage(true, result.Message),
                    "text/html");
            }

            return Content(
                BuildLeaveDecisionPage(false, result.Message, result.Errors),
                "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing leave approval from email");
            return Content(
                BuildLeaveDecisionPage(
                    false,
                    "This approval link could not be processed.",
                    new[] { "The link may be invalid or expired." }),
                "text/html");
        }
    }

    [HttpGet("reviewer/{reviewerId:int}/requests")]
    public async Task<IActionResult> GetReviewerRequests(
        int reviewerId,
        CancellationToken cancellationToken)
    {
        var reviewer = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reviewerId && x.IsActive, cancellationToken);

        if (reviewer == null)
        {
            return NotFound(new { Message = "Reviewer not found." });
        }

        var requests = await _context.LeaveApplications
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(x => x.PrimaryTeam)
            .Include(x => x.Employee)
                .ThenInclude(x => x.OrganizationRole)
            .Include(x => x.LeaveType)
            .Where(x => x.ApproverId == reviewerId && x.Status == "Pending")
            .OrderByDescending(x => x.AppliedOn ?? DateTime.UtcNow)
            .ThenBy(x => x.FromDate)
            .Select(x => new
            {
                x.Id,
                x.EmployeeId,
                EmployeeName = x.Employee.FullName,
                x.Employee.EmployeeCode,
                EmployeeRole = x.Employee.Role.GetDisplayName(),
                PrimaryTeamName = x.Employee.PrimaryTeam != null ? x.Employee.PrimaryTeam.Name : null,
                LeaveTypeName = x.LeaveType.Name,
                x.FromDate,
                x.ToDate,
                x.Reason,
                x.ApprovalReason,
                x.AppliedOn,
                TotalDays = x.TotalDays > 0
                    ? x.TotalDays
                    : EF.Functions.DateDiffDay(x.FromDate, x.ToDate) + 1
            })
            .ToListAsync(cancellationToken);

        var recentDecisions = await _context.LeaveApplications
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(x => x.PrimaryTeam)
            .Include(x => x.LeaveType)
            .Where(x => x.ApproverId == reviewerId && x.Status != "Pending")
            .OrderByDescending(x => x.Status == "Cancelled"
                ? x.RejectedOn ?? x.AppliedOn ?? DateTime.UtcNow
                : x.ApprovedOn ?? x.RejectedOn ?? x.AppliedOn ?? DateTime.UtcNow)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                x.EmployeeId,
                EmployeeName = x.Employee.FullName,
                x.Employee.EmployeeCode,
                EmployeeRole = x.Employee.Role.GetDisplayName(),
                PrimaryTeamName = x.Employee.PrimaryTeam != null ? x.Employee.PrimaryTeam.Name : null,
                LeaveTypeName = x.LeaveType.Name,
                x.FromDate,
                x.ToDate,
                x.Reason,
                x.ApprovalReason,
                x.AppliedOn,
                x.ApprovedOn,
                x.RejectedOn,
                ActionedOn = x.Status == "Cancelled"
                    ? x.RejectedOn
                    : x.ApprovedOn ?? x.RejectedOn,
                x.Status,
                TotalDays = x.TotalDays > 0
                    ? x.TotalDays
                    : EF.Functions.DateDiffDay(x.FromDate, x.ToDate) + 1
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            Reviewer = new
            {
                reviewer.Id,
                reviewer.FullName,
                Role = reviewer.Role.GetDisplayName()
            },
            Requests = requests,
            RecentDecisions = recentDecisions
        });
    }

    [HttpPost("reviewer/decision")]
    [ProducesResponseType(typeof(ApproveLeaveResponse), 200)]
    [ProducesResponseType(typeof(ApproveLeaveResponse), 400)]
    public async Task<IActionResult> ReviewLeave(
        [FromBody] ApproveLeaveCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _approveLeaveHandler.Handle(command, cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("{leaveApplicationId:int}/cancel")]
    public async Task<IActionResult> CancelLeave(
        int leaveApplicationId,
        [FromBody] CancelLeaveRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0)
        {
            return BadRequest(new { Message = "Employee is required to cancel a leave request." });
        }

        var leave = await _context.LeaveApplications
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == leaveApplicationId, cancellationToken);

        if (leave == null)
        {
            return NotFound(new { Message = "Leave request was not found." });
        }

        if (leave.EmployeeId != request.EmployeeId)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { Message = "Only the employee who applied this leave can cancel it." });
        }

        if (leave.Status != "Pending" && leave.Status != "Approved")
        {
            return BadRequest(new { Message = $"Leave request is already {leave.Status.ToLowerInvariant()} and cannot be cancelled." });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var restoredDays = 0m;

        if (leave.Status == "Approved")
        {
            var days = ResolveTotalDays(leave);
            var balance = await _context.EmployeeLeaveBalances
                .FirstOrDefaultAsync(
                    x => x.EmployeeId == leave.EmployeeId &&
                         x.LeaveTypeId == leave.LeaveTypeId,
                    cancellationToken);

            if (balance != null)
            {
                balance.UsedLeaves = Math.Max(0, balance.UsedLeaves - days);
                restoredDays = days;
            }
        }

        leave.Status = "Cancelled";
        leave.ApprovalReason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Cancelled by employee"
            : "Cancelled by employee: " + request.Reason.Trim();
        leave.RejectedOn = DateTime.UtcNow;
        leave.RejectedById = request.EmployeeId;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var message = restoredDays > 0
            ? $"Leave request cancelled. {restoredDays} days restored to the employee balance."
            : "Leave request cancelled.";

        return Ok(new
        {
            Success = true,
            Message = message,
            RestoredDays = restoredDays
        });
    }

    private static decimal ResolveTotalDays(LeaveApplication leave)
    {
        if (leave.TotalDays > 0)
        {
            return leave.TotalDays;
        }

        return (decimal)((leave.ToDate.Date - leave.FromDate.Date).TotalDays + 1);
    }

    private static string BuildLeaveDecisionPage(
        bool success,
        string message,
        IEnumerable<string>? details = null)
    {
        var title = success ? "Leave Request Approved" : "Leave Request Not Completed";
        var statusClass = success ? "success" : "error";
        var safeMessage = WebUtility.HtmlEncode(message);
        var detailItems = details == null
            ? string.Empty
            : string.Join(
                string.Empty,
                details
                    .Where(detail => !string.IsNullOrWhiteSpace(detail))
                    .Select(detail => $"<li>{WebUtility.HtmlEncode(detail)}</li>"));

        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>{{title}}</title>
    <style>
        body {
            margin: 0;
            min-height: 100vh;
            display: grid;
            place-items: center;
            padding: 24px;
            background: #f7f2ff;
            color: #24302b;
            font-family: Arial, sans-serif;
        }
        .panel {
            width: min(560px, 100%);
            padding: 28px;
            border: 1px solid #e7def2;
            border-radius: 8px;
            background: #ffffff;
            box-shadow: 0 20px 60px rgba(36, 28, 48, 0.12);
        }
        .badge {
            display: inline-flex;
            min-height: 32px;
            align-items: center;
            padding: 0 12px;
            border-radius: 999px;
            font-size: 0.78rem;
            font-weight: 800;
            text-transform: uppercase;
        }
        .badge.success {
            color: #245c3d;
            background: #dbf3e5;
        }
        .badge.error {
            color: #8c2f24;
            background: #f8dfda;
        }
        h1 {
            margin: 18px 0 10px;
            font-size: 1.6rem;
        }
        p, li {
            color: #5f6f68;
            line-height: 1.55;
        }
        ul {
            margin: 16px 0 0;
            padding-left: 20px;
        }
        a {
            display: inline-flex;
            align-items: center;
            min-height: 42px;
            margin-top: 18px;
            padding: 0 16px;
            border-radius: 8px;
            color: #ffffff;
            background: #315f4f;
            font-weight: 800;
            text-decoration: none;
        }
    </style>
</head>
<body>
    <main class="panel">
        <span class="badge {{statusClass}}">{{(success ? "Completed" : "Needs Attention")}}</span>
        <h1>{{title}}</h1>
        <p>{{safeMessage}}</p>
        {{(string.IsNullOrWhiteSpace(detailItems) ? string.Empty : $"<ul>{detailItems}</ul>")}}
        <a href="/">Open LeaveManager</a>
    </main>
</body>
</html>
""";
    }
}

public class CancelLeaveRequest
{
    public int EmployeeId { get; set; }

    public string? Reason { get; set; }
}
