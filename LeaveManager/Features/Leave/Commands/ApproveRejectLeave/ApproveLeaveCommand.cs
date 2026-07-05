namespace LeaveManager.Features.Leave.Commands.ApproveRejectLeave;

public class ApproveLeaveCommand
{
    public int LeaveApplicationId { get; set; }
    public int ApproverId { get; set; }
    public string Action { get; set; } = string.Empty; // "approve" or "reject"
    public string? Reason { get; set; }
}
