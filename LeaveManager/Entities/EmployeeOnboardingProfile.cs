namespace LeaveManager.Entities;

public class EmployeeOnboardingProfile
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public string PanNumber { get; set; } = string.Empty;

    public string AadhaarNumber { get; set; } = string.Empty;

    public bool HasPriorExperience { get; set; }

    public string? PreviousEmployerName { get; set; }

    public decimal? YearsOfExperience { get; set; }

    public bool RelievingEmailForwarded { get; set; }

    public DateTime LastUpdatedOn { get; set; }

    public ICollection<EmployeeOnboardingDocument> Documents { get; set; } = new List<EmployeeOnboardingDocument>();
}
