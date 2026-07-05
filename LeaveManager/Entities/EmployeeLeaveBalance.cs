using LeaveManager.Entities;

public class EmployeeLeaveBalance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    public decimal AllocatedLeaves { get; set; }

    public decimal UsedLeaves { get; set; }
}