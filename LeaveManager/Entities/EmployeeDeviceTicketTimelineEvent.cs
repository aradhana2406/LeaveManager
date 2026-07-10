namespace LeaveManager.Entities;

public class EmployeeDeviceTicketTimelineEvent
{
    public int Id { get; set; }

    public int EmployeeDeviceTicketId { get; set; }

    public EmployeeDeviceTicket EmployeeDeviceTicket { get; set; } = null!;

    public string Status { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}
