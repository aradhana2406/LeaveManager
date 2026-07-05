using LeaveManager.Data;
using LeaveManager.Entities;
using LeaveManager.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Features.Leave.Commands.ApproveRejectLeave;

public class ApproveLeaveHandler
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ApproveLeaveHandler> _logger;

    public ApproveLeaveHandler(
        AppDbContext context,
        IEmailService emailService,
        ILogger<ApproveLeaveHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ApproveLeaveResponse> Handle(
        ApproveLeaveCommand request,
        CancellationToken cancellationToken)
    {
        var response = new ApproveLeaveResponse();

        try
        {
            _logger.LogInformation("Processing leave approval: LeaveAppId={LeaveAppId}, Action={Action}",
                request.LeaveApplicationId, request.Action);

            var leave = await _context.LeaveApplications
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .Include(x => x.Approver)
                .FirstOrDefaultAsync(
                    x => x.Id == request.LeaveApplicationId,
                    cancellationToken);

            if (leave == null)
            {
                response.Success = false;
                response.Errors.Add($"Leave application with ID {request.LeaveApplicationId} not found");
                response.Message = "Leave application not found.";
                _logger.LogWarning("Leave application not found: {LeaveAppId}", request.LeaveApplicationId);
                return response;
            }

            if (leave.ApproverId != request.ApproverId)
            {
                response.Success = false;
                response.Errors.Add("Unauthorized - You are not the assigned approver for this request");
                response.Message = "Unauthorized approver.";
                _logger.LogWarning("Unauthorized approver attempt: LeaveAppId={LeaveAppId}, AttemptedApproverId={AttemptedApproverId}, AssignedApproverId={AssignedApproverId}",
                    request.LeaveApplicationId, request.ApproverId, leave.ApproverId);
                return response;
            }

            if (leave.Status != "Pending")
            {
                response.Success = false;
                response.Errors.Add($"Leave request is already {leave.Status} and cannot be modified");
                response.Message = "Cannot modify already processed request.";
                _logger.LogWarning("Attempting to modify non-pending leave: LeaveAppId={LeaveAppId}, CurrentStatus={Status}",
                    request.LeaveApplicationId, leave.Status);
                return response;
            }

            if (!request.Action.Equals("approve", StringComparison.OrdinalIgnoreCase) &&
                !request.Action.Equals("reject", StringComparison.OrdinalIgnoreCase))
            {
                response.Success = false;
                response.Errors.Add("Invalid action. Must be 'approve' or 'reject'");
                response.Message = "Invalid action.";
                return response;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.Action.Equals("approve", StringComparison.OrdinalIgnoreCase))
                {
                    leave.Status = "Approved";
                    leave.ApprovedOn = DateTime.UtcNow;
                    leave.ApprovedById = request.ApproverId;
                    leave.ApprovalReason = request.Reason ?? "Approved";

                    var days = leave.TotalDays > 0
                        ? leave.TotalDays
                        : (decimal)((leave.ToDate - leave.FromDate).Days + 1);

                    var balance = await _context.EmployeeLeaveBalances
                        .FirstOrDefaultAsync(
                            x => x.EmployeeId == leave.EmployeeId &&
                                 x.LeaveTypeId == leave.LeaveTypeId,
                            cancellationToken);

                    if (balance != null)
                    {
                        balance.UsedLeaves += days;
                        _logger.LogInformation("Updated leave balance: EmployeeId={EmployeeId}, LeaveTypeId={LeaveTypeId}, UsedLeaves={UsedLeaves}",
                            leave.EmployeeId, leave.LeaveTypeId, balance.UsedLeaves);
                    }

                    response.Message = $"Leave approved successfully. {days} days marked as used.";
                }
                else
                {
                    leave.Status = "Rejected";
                    leave.RejectedOn = DateTime.UtcNow;
                    leave.RejectedById = request.ApproverId;
                    leave.ApprovalReason = request.Reason ?? "Rejected by approver";

                    response.Message = "Leave rejected successfully.";
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                try
                {
                    await SendNotificationToEmployee(leave, cancellationToken);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send notification email to employee {EmployeeId}",
                        leave.EmployeeId);
                }

                response.Success = true;
                _logger.LogInformation("Leave {Action} successful: LeaveAppId={LeaveAppId}, EmployeeId={EmployeeId}",
                    request.Action, request.LeaveApplicationId, leave.EmployeeId);

                return response;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(dbEx, "Database error while processing leave approval: LeaveAppId={LeaveAppId}",
                    request.LeaveApplicationId);
                response.Success = false;
                response.Errors.Add("Database error occurred while saving changes");
                response.Message = "Database error.";
                return response;
            }
            catch (Exception txEx)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(txEx, "Transaction error while processing leave approval: LeaveAppId={LeaveAppId}",
                    request.LeaveApplicationId);
                response.Success = false;
                response.Errors.Add("Transaction error occurred");
                response.Message = "Transaction failed.";
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ApproveLeaveHandler");
            response.Success = false;
            response.Errors.Add("An unexpected error occurred");
            response.Message = "Internal error.";
            return response;
        }
    }

    private async Task SendNotificationToEmployee(LeaveApplication leave, CancellationToken cancellationToken)
    {
        var statusText = leave.Status == "Approved" ? "APPROVED" : "REJECTED";
        var statusColor = leave.Status == "Approved" ? "#27ae60" : "#e74c3c";
        var days = leave.TotalDays > 0
            ? leave.TotalDays
            : (decimal)((leave.ToDate - leave.FromDate).Days + 1);

        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {statusColor}; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ border: 1px solid #ddd; padding: 20px; }}
        .table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        .table td {{ padding: 10px; border: 1px solid #ddd; }}
        .table tr:nth-child(even) {{ background-color: #f9f9f9; }}
        .footer {{ background-color: #ecf0f1; padding: 15px; text-align: center; font-size: 12px; color: #7f8c8d; border-radius: 0 0 5px 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Leave Request {statusText}</h2>
        </div>
        <div class='content'>
            <p>Dear {leave.Employee?.FullName},</p>
            <p>Your leave request has been <b>{leave.Status.ToLower()}</b> by {leave.Approver?.FullName}.</p>

            <table class='table'>
                <tr><td><b>Leave Type</b></td><td>{leave.LeaveType?.Name}</td></tr>
                <tr><td><b>From Date</b></td><td>{leave.FromDate:dd-MMM-yyyy}</td></tr>
                <tr><td><b>To Date</b></td><td>{leave.ToDate:dd-MMM-yyyy}</td></tr>
                <tr><td><b>Total Days</b></td><td>{days}</td></tr>
            </table>

            {(string.IsNullOrEmpty(leave.ApprovalReason) ? "" : $"<p><b>Reason:</b> {leave.ApprovalReason}</p>")}

            <p>Best regards,<br/><b>Leave Management System</b></p>
        </div>
        <div class='footer'>
            <p>Please do not reply to this email</p>
        </div>
    </div>
</body>
</html>";

        await _emailService.SendEmailAsync(
            leave.Employee!.Email,
            $"Leave Request {leave.Status}",
            emailBody);
    }
}
