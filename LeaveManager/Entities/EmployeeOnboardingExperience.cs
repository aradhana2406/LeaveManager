namespace LeaveManager.Entities;

public class EmployeeOnboardingExperience
{
    public int Id { get; set; }

    public int EmployeeOnboardingProfileId { get; set; }

    public EmployeeOnboardingProfile EmployeeOnboardingProfile { get; set; } = null!;

    public string CompanyName { get; set; } = string.Empty;

    public string? JobTitle { get; set; }

    public decimal? YearsOfExperience { get; set; }

    public bool RelievingEmailForwarded { get; set; }

    public ICollection<EmployeeOnboardingDocument> Documents { get; set; } = new List<EmployeeOnboardingDocument>();
}
