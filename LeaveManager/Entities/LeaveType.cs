namespace LeaveManager.Entities;

public class LeaveType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsPaid { get; set; }

    public bool RequiresAdvanceNotice { get; set; }

    public int AdvanceNoticeDays { get; set; }

    public bool IsAccrued { get; set; } = false;

    public decimal AccrualPerMonth { get; set; } = 0;

    public ICollection<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; } = new List<EmployeeLeaveBalance>();
}
