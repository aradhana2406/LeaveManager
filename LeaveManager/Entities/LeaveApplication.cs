using LeaveManager.Entities;

public class LeaveApplication
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public decimal TotalDays { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = "Pending";

    public int ApproverId { get; set; }
    public Employee? Approver { get; set; }

    public DateTime? AppliedOn { get; set; }
    public DateTime? ApprovedOn { get; set; }
    public DateTime? RejectedOn { get; set; }

    /// <summary>
    /// Comments/reason for approval or rejection
    /// </summary>
    public string? ApprovalReason { get; set; }

    /// <summary>
    /// Employee who approved this leave (if approved)
    /// </summary>
    public int? ApprovedById { get; set; }

    /// <summary>
    /// Employee who rejected this leave (if rejected)
    /// </summary>
    public int? RejectedById { get; set; }
}
