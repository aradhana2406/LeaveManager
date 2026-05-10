namespace LeaveManager.Entities;

public class LeaveApplication
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int TotalDays { get; set; }

    public string Status { get; set; } = "Pending";

    public string? Reason { get; set; }
    public int ApproverId { get; set; }
}