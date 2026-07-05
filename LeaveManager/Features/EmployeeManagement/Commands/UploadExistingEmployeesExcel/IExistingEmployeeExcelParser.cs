namespace LeaveManager.Features.EmployeeManagement.Commands.UploadExistingEmployeesExcel;

public interface IExistingEmployeeExcelParser
{
    Task<List<ExistingEmployeeExcelRow>> ParseAsync(IFormFile file);
}
