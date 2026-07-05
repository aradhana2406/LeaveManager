namespace LeaveManager.Features.EmployeeManagement.Services;

public interface IOnboardingFileStorageService
{
    Task<(string StoredFileName, string RelativePath)> SaveAsync(
        IFormFile file,
        int employeeId,
        string documentType,
        CancellationToken cancellationToken);

    string GetAbsolutePath(string relativePath);
}
