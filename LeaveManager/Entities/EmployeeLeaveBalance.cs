namespace LeaveManager.Entities;

public class EmployeeLeaveBalance
{
    public int Id { get; set; }

    public int EmployeeId{ get; set; }

    public int LeaveTypeId { get; set; }

    public int AllocatedLeaves { get; set; }

    public int UsedLeaves { get; set; }
    public Employee? Employee { get; set; }
    public LeaveType? LeaveType { get; set; }

}