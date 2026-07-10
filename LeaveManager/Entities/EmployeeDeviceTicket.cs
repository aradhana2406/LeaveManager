namespace LeaveManager.Entities;

public class EmployeeDeviceTicket
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int? AssignedHrId { get; set; }

    public Employee? AssignedHr { get; set; }

    public string RequestType { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;

    public string NotificationTo { get; set; } = "devicehelp@company.com";

    public string NotificationCc { get; set; } = "hr@company.com";

    public string Subject { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Submitted";

    public DateTime CreatedOn { get; set; }

    public DateTime LastUpdatedOn { get; set; }

    public ICollection<EmployeeDeviceTicketTimelineEvent> TimelineEvents { get; set; } = new List<EmployeeDeviceTicketTimelineEvent>();
}
