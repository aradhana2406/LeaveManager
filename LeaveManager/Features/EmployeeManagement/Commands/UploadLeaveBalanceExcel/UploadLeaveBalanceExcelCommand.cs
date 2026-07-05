namespace LeaveManager.Features.EmployeeManagement.Commands.UploadLeaveBalanceExcel;

public class UploadLeaveBalanceExcelCommand
{
    public IFormFile File { get; set; }

    public UploadLeaveBalanceExcelCommand(IFormFile file)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
    }
}
