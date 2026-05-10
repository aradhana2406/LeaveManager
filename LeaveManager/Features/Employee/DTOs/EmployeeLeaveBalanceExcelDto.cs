namespace LeaveManager.Features.Employee.DTOs;

public class EmployeeLeaveBalanceExcelDto
{
    public string EmployeeCode { get; set; } = string.Empty;

    public string LeaveType { get; set; } = string.Empty;

    public int AllocatedLeaves { get; set; }
}