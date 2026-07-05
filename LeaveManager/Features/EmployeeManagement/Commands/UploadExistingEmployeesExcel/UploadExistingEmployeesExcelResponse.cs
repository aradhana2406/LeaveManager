namespace LeaveManager.Features.EmployeeManagement.Commands.UploadExistingEmployeesExcel;

public class UploadExistingEmployeesExcelResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int RecordsProcessed { get; set; }

    public int RecordsFailed { get; set; }

    public int RecordsSkipped { get; set; }

    public List<string> Errors { get; set; } = new();
}
