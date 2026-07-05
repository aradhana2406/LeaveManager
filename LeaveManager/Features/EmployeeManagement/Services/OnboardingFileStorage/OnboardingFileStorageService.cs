namespace LeaveManager.Features.EmployeeManagement.Services;

public class OnboardingFileStorageService : IOnboardingFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public OnboardingFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(string StoredFileName, string RelativePath)> SaveAsync(
        IFormFile file,
        int employeeId,
        string documentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        var safeDocumentType = documentType.Replace(" ", string.Empty);
        var storedFileName = $"{safeDocumentType}_{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine("App_Data", "OnboardingFiles", employeeId.ToString(), storedFileName);
        var absolutePath = GetAbsolutePath(relativePath);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return (storedFileName, relativePath);
    }

    public string GetAbsolutePath(string relativePath)
    {
        return Path.Combine(_environment.ContentRootPath, relativePath);
    }
}
