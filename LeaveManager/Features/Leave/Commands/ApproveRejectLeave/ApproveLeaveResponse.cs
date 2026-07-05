namespace LeaveManager.Features.Leave.Commands.ApproveRejectLeave;

public class ApproveLeaveResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}