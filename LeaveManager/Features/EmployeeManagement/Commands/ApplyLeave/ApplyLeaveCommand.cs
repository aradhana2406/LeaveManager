namespace LeaveManager.Features.EmployeeManagement.Commands.ApplyLeave;

public class ApplyLeaveCommand
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsHalfDay { get; set; }
    public string Reason { get; set; } = string.Empty;

    public decimal DaysRequested()
    {
        var calendarDays = (int)(EndDate.Date - StartDate.Date).TotalDays + 1;

        if (IsHalfDay)
        {
            return calendarDays * 0.5m;
        }

        return calendarDays;
    }
}
