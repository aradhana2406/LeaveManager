namespace LeaveManager.Features.EmployeeManagement.Commands.ApplyLeave;

public class ApplyLeaveResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? LeaveApplicationId { get; set; }
    public List<string> Errors { get; set; } = new();
}