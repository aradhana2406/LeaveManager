namespace LeaveManager.Entities;

public class EmployeeOnboardingDocument
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int? EmployeeOnboardingExperienceId { get; set; }

    public EmployeeOnboardingExperience? EmployeeOnboardingExperience { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public DateTime UploadedOn { get; set; }
}
