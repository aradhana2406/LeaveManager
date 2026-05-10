using MediatR;

namespace LeaveManager.Features.Employee.Commands.UploadLeaveBalanceExcel;

public class UploadLeaveBalanceExcelCommand : IRequest<string>
{
    public IFormFile File { get; set; }

    public UploadLeaveBalanceExcelCommand(IFormFile file)
    {
        File = file;
    }
}
