namespace LeaveManager.Features.EmployeeManagement.Commands.UploadExistingEmployeesExcel;

public class ExistingEmployeeExcelRow
{
    public int RowNumber { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string OfficialEmail { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public string JobRole { get; set; } = string.Empty;

    public DateTime JoinDate { get; set; }

    public string EmploymentType { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string PrimaryTeam { get; set; } = string.Empty;

    public string AdditionalTeams { get; set; } = string.Empty;

    public string TeamLeadEmpcode { get; set; } = string.Empty;

    public string SalaryStructureDetails { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string TemporaryPassword { get; set; } = string.Empty;
}
