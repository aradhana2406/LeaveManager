using LeaveManager.Common.Enums;
using LeaveManager.Data;
using LeaveManager.Entities;
using LeaveManager.Features.EmployeeManagement.Services;
using LeaveManager.Infrastructure.Notifications;
using LeaveManager.Infrastructure.Token;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Features.EmployeeManagement.Commands.ApplyLeave;

public class ApplyLeaveHandler
{
    private readonly AppDbContext _context;
    private readonly ILeaveAccrualService _leaveAccrualService;
    private readonly IEmailService _emailService;
    private readonly IApprovalTokenService _tokenService;
    private readonly ILogger<ApplyLeaveHandler> _logger;
    private readonly IConfiguration _configuration;

    public ApplyLeaveHandler(
        AppDbContext context,
        ILeaveAccrualService leaveAccrualService,
        IEmailService emailService,
        IApprovalTokenService tokenService,
        ILogger<ApplyLeaveHandler> logger,
        IConfiguration configuration)
    {
        _context = context;
        _leaveAccrualService = leaveAccrualService;
        _emailService = emailService;
        _tokenService = tokenService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ApplyLeaveResponse> Handle(
        ApplyLeaveCommand request,
        CancellationToken cancellationToken)
    {
        var response = new ApplyLeaveResponse();

        try
        {
            var employee = await _context.Employees
                .Include(x => x.PrimaryTeam)
                    .ThenInclude(x => x!.Lead)
                .FirstOrDefaultAsync(
                    x => x.Id == request.EmployeeId && x.IsActive,
                    cancellationToken);

            if (employee == null)
            {
                response.Success = false;
                response.Errors.Add($"Employee with ID {request.EmployeeId} not found or inactive");
                response.Message = "Employee validation failed.";
                return response;
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(
                    x => x.Id == request.LeaveTypeId,
                    cancellationToken);

            if (leaveType == null)
            {
                response.Success = false;
                response.Errors.Add($"Leave type with ID {request.LeaveTypeId} not found");
                response.Message = "Leave type validation failed.";
                return response;
            }

            var days = request.DaysRequested();

            if (days <= 0)
            {
                response.Success = false;
                response.Errors.Add("End date must be on or after start date");
                response.Message = "Date validation failed.";
                return response;
            }

            if (request.IsHalfDay)
            {
                var policy = await _context.HrPolicies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (policy?.AllowHalfDayLeave != true)
                {
                    response.Success = false;
                    response.Errors.Add("Half day leave is currently disabled by HR.");
                    response.Message = "Half day leave is not allowed.";
                    return response;
                }
            }

            if (leaveType.Name != "Unpaid Leave")
            {
                await _leaveAccrualService.SyncEmployeeAsync(request.EmployeeId, cancellationToken);

                var balance = await _context.EmployeeLeaveBalances
                    .FirstOrDefaultAsync(
                        x => x.EmployeeId == request.EmployeeId &&
                             x.LeaveTypeId == request.LeaveTypeId,
                        cancellationToken);

                if (balance == null)
                {
                    response.Success = false;
                    response.Errors.Add($"No leave balance found for {leaveType.Name}");
                    response.Message = "Leave balance validation failed.";
                    return response;
                }

                var remaining = balance.AllocatedLeaves - balance.UsedLeaves;

                if (remaining < days)
                {
                    response.Success = false;
                    response.Errors.Add(
                        $"Insufficient balance. Available: {remaining} days, Requested: {days} days");
                    response.Message = "Insufficient leave balance.";
                    return response;
                }
            }

            var approver = await ResolveApproverAsync(employee, cancellationToken);

            if (approver == null)
            {
                response.Success = false;
                response.Errors.Add("No approver could be resolved for this employee");
                response.Message = "Approver validation failed.";
                return response;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var leave = new LeaveApplication
                {
                    EmployeeId = request.EmployeeId,
                    LeaveTypeId = request.LeaveTypeId,
                    FromDate = request.StartDate,
                    ToDate = request.EndDate,
                    TotalDays = days,
                    Reason = request.Reason,
                    Status = "Pending",
                    ApproverId = approver.Id,
                    AppliedOn = DateTime.UtcNow
                };

                await _context.LeaveApplications.AddAsync(leave, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var approvalToken = _tokenService.GenerateToken(leave.Id, approver.Id);
                var appBaseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://yourapp.com";

                try
                {
                    await _emailService.SendEmailAsync(
                        approver.Email,
                        "New Leave Request - Action Required",
                        GenerateLeaveRequestEmailBody(employee, approver, leaveType, request, days, approvalToken, appBaseUrl));
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send email to approver {ApproverId}", approver.Id);
                }

                await transaction.CommitAsync(cancellationToken);

                response.Success = true;
                response.LeaveApplicationId = leave.Id;
                response.Message = $"Leave application submitted successfully for approval to {approver.FullName}";
                return response;
            }
            catch (Exception txEx)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(txEx, "Transaction error while creating leave application for employee {EmployeeId}",
                    request.EmployeeId);
                response.Success = false;
                response.Errors.Add("Failed to save leave application. Please try again.");
                response.Message = "Database error.";
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ApplyLeaveHandler for employee {EmployeeId}",
                request.EmployeeId);
            response.Success = false;
            response.Errors.Add("An unexpected error occurred. Please contact support.");
            response.Message = "Internal error.";
            return response;
        }
    }

    private async Task<Employee?> ResolveApproverAsync(Employee employee, CancellationToken cancellationToken)
    {
        if (employee.Role == Role.OrganizationHead)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(
                    x => x.Role.IsHrRole() && x.IsActive,
                    cancellationToken);
        }

        if (employee.Role.IsHrRole())
        {
            return await _context.Employees
                .FirstOrDefaultAsync(
                    x => x.Role == Role.OrganizationHead && x.IsActive,
                    cancellationToken);
        }

        if (employee.Role.IsManagerRole())
        {
            return await _context.Employees
                .FirstOrDefaultAsync(
                    x => x.Role.IsHrRole() && x.IsActive,
                    cancellationToken);
        }

        return employee.PrimaryTeam?.Lead is { IsActive: true }
            ? employee.PrimaryTeam.Lead
            : null;
    }

    private static string GenerateLeaveRequestEmailBody(
        Employee employee,
        Employee approver,
        LeaveType leaveType,
        ApplyLeaveCommand request,
        decimal days,
        string approvalToken,
        string baseUrl)
    {
        var approveUrl = $"{baseUrl}/api/leave/approve?token={approvalToken}&action=approve";
        var rejectUrl = $"{baseUrl}/api/leave/approve?token={approvalToken}&action=reject";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ border: 1px solid #ddd; padding: 20px; }}
        .table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .table td {{ padding: 10px; border: 1px solid #ddd; }}
        .table tr:nth-child(even) {{ background-color: #f9f9f9; }}
        .label {{ font-weight: bold; width: 30%; }}
        .buttons {{ margin: 30px 0; text-align: center; }}
        .btn {{ padding: 12px 30px; margin: 0 10px; font-size: 16px; border: none; border-radius: 5px; cursor: pointer; text-decoration: none; display: inline-block; }}
        .btn-approve {{ background-color: #27ae60; color: white; }}
        .btn-reject {{ background-color: #e74c3c; color: white; }}
        .btn-approve:hover {{ background-color: #229954; }}
        .btn-reject:hover {{ background-color: #c0392b; }}
        .footer {{ background-color: #ecf0f1; padding: 15px; text-align: center; font-size: 12px; color: #7f8c8d; border-radius: 0 0 5px 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Leave Request - Awaiting Your Approval</h2>
        </div>
        <div class='content'>
            <p>Dear <b>{approver.FullName}</b>,</p>
            <p><b>{employee.FullName}</b> ({employee.EmployeeCode}) has applied for leave.</p>
            <table class='table'>
                <tr><td class='label'>Leave Type</td><td>{leaveType.Name}</td></tr>
                <tr><td class='label'>From Date</td><td>{request.StartDate:dd-MMM-yyyy}</td></tr>
                <tr><td class='label'>To Date</td><td>{request.EndDate:dd-MMM-yyyy}</td></tr>
                <tr><td class='label'>Leave Length</td><td>{(request.IsHalfDay ? "Half day for each selected date" : "Full day")}</td></tr>
                <tr><td class='label'>Total Days</td><td><b>{days}</b></td></tr>
                <tr><td class='label'>Reason</td><td>{request.Reason}</td></tr>
            </table>
            <div class='buttons'>
                <a href='{approveUrl}' class='btn btn-approve'>Approve</a>
                <a href='{rejectUrl}' class='btn btn-reject'>Reject</a>
            </div>
        </div>
        <div class='footer'>
            <p>Leave Management System | Do not reply to this email</p>
        </div>
    </div>
</body>
</html>";
    }
}
