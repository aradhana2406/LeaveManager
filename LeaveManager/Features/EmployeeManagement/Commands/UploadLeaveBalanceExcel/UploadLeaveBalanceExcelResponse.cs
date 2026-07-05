namespace LeaveManager.Features.EmployeeManagement.Commands.UploadLeaveBalanceExcel;

public class UploadLeaveBalanceExcelResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public List<string> Errors { get; set; } = new();
}