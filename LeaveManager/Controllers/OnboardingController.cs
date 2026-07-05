using LeaveManager.Data;
using LeaveManager.Entities;
using LeaveManager.Features.EmployeeManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Controllers;

[ApiController]
[Route("api/onboarding")]
public class OnboardingController : ControllerBase
{
    private const string ExperienceLetterDocumentType = "Experience Letter";
    private const string SalarySlipDocumentType = "Salary Slip";
    private const string AdditionalDocumentType = "Additional Document";

    private readonly AppDbContext _context;
    private readonly IOnboardingFileStorageService _fileStorageService;

    public OnboardingController(
        AppDbContext context,
        IOnboardingFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetProfile(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == employeeId && x.IsActive, cancellationToken);

        if (employee == null)
        {
            return NotFound(new { Message = "Employee not found." });
        }

        var profile = await _context.EmployeeOnboardingProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

        var documents = await _context.EmployeeOnboardingDocuments
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.UploadedOn)
            .Select(x => new
            {
                x.Id,
                x.DocumentType,
                x.OriginalFileName,
                x.UploadedOn
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            Employee = new
            {
                employee.Id,
                employee.EmployeeCode,
                employee.FullName,
                employee.Email,
                employee.Department,
                employee.Designation,
                employee.JobRole,
                employee.EmploymentType,
                employee.Location,
                employee.JoinDate
            },
            Profile = profile == null
                ? null
                : new
                {
                    profile.EmployeeId,
                    profile.PanNumber,
                    profile.AadhaarNumber,
                    profile.HasPriorExperience,
                    profile.PreviousEmployerName,
                    profile.YearsOfExperience,
                    profile.RelievingEmailForwarded,
                    profile.LastUpdatedOn
                },
            Documents = documents
        });
    }

    [HttpPost]
    [RequestSizeLimit(52428800)]
    public async Task<IActionResult> SaveProfile(
        [FromForm] SaveEmployeeOnboardingRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0)
        {
            return BadRequest(new { Message = "Employee is required." });
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId && x.IsActive, cancellationToken);

        if (employee == null)
        {
            return BadRequest(new { Message = "Employee not found." });
        }

        var panNumber = request.PanNumber.Trim().ToUpperInvariant();
        var aadhaarNumber = request.AadhaarNumber.Replace(" ", string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(panNumber) || panNumber.Length < 10)
        {
            return BadRequest(new { Message = "PAN number is required." });
        }

        if (string.IsNullOrWhiteSpace(aadhaarNumber) || aadhaarNumber.Length < 12)
        {
            return BadRequest(new { Message = "Aadhaar number is required." });
        }

        if (request.HasPriorExperience &&
            request.SalarySlips.Count == 0 &&
            request.ExperienceLetter == null)
        {
            return BadRequest(new { Message = "Upload at least one prior-experience document." });
        }

        var profile = await _context.EmployeeOnboardingProfiles
            .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId, cancellationToken);

        if (profile == null)
        {
            profile = new EmployeeOnboardingProfile
            {
                EmployeeId = request.EmployeeId
            };

            await _context.EmployeeOnboardingProfiles.AddAsync(profile, cancellationToken);
        }

        profile.PanNumber = panNumber;
        profile.AadhaarNumber = aadhaarNumber;
        profile.HasPriorExperience = request.HasPriorExperience;
        profile.PreviousEmployerName = request.HasPriorExperience
            ? request.PreviousEmployerName?.Trim()
            : null;
        profile.YearsOfExperience = request.HasPriorExperience
            ? request.YearsOfExperience
            : null;
        profile.RelievingEmailForwarded = request.HasPriorExperience && request.RelievingEmailForwarded;
        profile.LastUpdatedOn = DateTime.UtcNow;

        await SaveDocumentsAsync(request.EmployeeId, request.ExperienceLetter, ExperienceLetterDocumentType, cancellationToken);

        foreach (var slip in request.SalarySlips)
        {
            await SaveDocumentsAsync(request.EmployeeId, slip, SalarySlipDocumentType, cancellationToken);
        }

        foreach (var document in request.AdditionalDocuments)
        {
            await SaveDocumentsAsync(request.EmployeeId, document, AdditionalDocumentType, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Onboarding profile saved successfully." });
    }

    [HttpGet("documents/{documentId:int}")]
    public async Task<IActionResult> DownloadDocument(int documentId, CancellationToken cancellationToken)
    {
        var document = await _context.EmployeeOnboardingDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);

        if (document == null)
        {
            return NotFound();
        }

        var absolutePath = _fileStorageService.GetAbsolutePath(document.RelativePath);
        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound();
        }

        var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, document.ContentType, document.OriginalFileName);
    }

    private async Task SaveDocumentsAsync(
        int employeeId,
        IFormFile? file,
        string documentType,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return;
        }

        var stored = await _fileStorageService.SaveAsync(file, employeeId, documentType, cancellationToken);
        await _context.EmployeeOnboardingDocuments.AddAsync(new EmployeeOnboardingDocument
        {
            EmployeeId = employeeId,
            DocumentType = documentType,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = stored.StoredFileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            RelativePath = stored.RelativePath,
            UploadedOn = DateTime.UtcNow
        }, cancellationToken);
    }
}

public class SaveEmployeeOnboardingRequest
{
    public int EmployeeId { get; set; }

    public string PanNumber { get; set; } = string.Empty;

    public string AadhaarNumber { get; set; } = string.Empty;

    public bool HasPriorExperience { get; set; }

    public string? PreviousEmployerName { get; set; }

    public decimal? YearsOfExperience { get; set; }

    public bool RelievingEmailForwarded { get; set; }

    public IFormFile? ExperienceLetter { get; set; }

    public List<IFormFile> SalarySlips { get; set; } = new();

    public List<IFormFile> AdditionalDocuments { get; set; } = new();
}
