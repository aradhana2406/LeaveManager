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
            .Include(x => x.Experiences)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

        var documents = await _context.EmployeeOnboardingDocuments
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.UploadedOn)
            .Select(x => new
            {
                x.Id,
                x.DocumentType,
                x.EmployeeOnboardingExperienceId,
                ExperienceCompanyName = x.EmployeeOnboardingExperience == null
                    ? null
                    : x.EmployeeOnboardingExperience.CompanyName,
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
                    Experiences = profile.Experiences
                        .OrderBy(x => x.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.CompanyName,
                            x.JobTitle,
                            x.YearsOfExperience,
                            x.RelievingEmailForwarded
                        })
                        .ToList(),
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

        var experienceRows = request.Experiences
            .Where(x => !string.IsNullOrWhiteSpace(x.CompanyName))
            .Select(x => new SaveEmployeeOnboardingExperienceRequest
            {
                Id = x.Id,
                CompanyName = x.CompanyName.Trim(),
                JobTitle = x.JobTitle?.Trim(),
                YearsOfExperience = x.YearsOfExperience,
                RelievingEmailForwarded = x.RelievingEmailForwarded,
                ExperienceLetter = x.ExperienceLetter
            })
            .ToList();

        if (request.HasPriorExperience && experienceRows.Count == 0)
        {
            return BadRequest(new { Message = "Add at least one previous company." });
        }

        var profile = await _context.EmployeeOnboardingProfiles
            .Include(x => x.Experiences)
                .ThenInclude(x => x.Documents)
            .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId, cancellationToken);

        var existingExperienceIds = profile == null
            ? new HashSet<int>()
            : profile.Experiences.Select(x => x.Id).ToHashSet();

        var existingLetterExperienceIds = await _context.EmployeeOnboardingDocuments
            .AsNoTracking()
            .Where(x => x.EmployeeId == request.EmployeeId &&
                        x.DocumentType == ExperienceLetterDocumentType &&
                        x.EmployeeOnboardingExperienceId.HasValue)
            .Select(x => x.EmployeeOnboardingExperienceId!.Value)
            .ToListAsync(cancellationToken);

        if (request.HasPriorExperience)
        {
            var missingLetterCompanies = experienceRows
                .Where(x => x.ExperienceLetter == null &&
                            (x.Id <= 0 ||
                             !existingExperienceIds.Contains(x.Id) ||
                             !existingLetterExperienceIds.Contains(x.Id)))
                .Select(x => x.CompanyName)
                .ToList();

            if (missingLetterCompanies.Count > 0)
            {
                return BadRequest(new
                {
                    Message = "Upload an experience letter for each previous company: " +
                              string.Join(", ", missingLetterCompanies) + "."
                });
            }
        }

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
            ? experienceRows.FirstOrDefault()?.CompanyName ?? request.PreviousEmployerName?.Trim()
            : null;
        profile.YearsOfExperience = request.HasPriorExperience
            ? experienceRows.Sum(x => x.YearsOfExperience ?? 0m)
            : null;
        profile.RelievingEmailForwarded = request.HasPriorExperience && experienceRows.Any(x => x.RelievingEmailForwarded);
        profile.LastUpdatedOn = DateTime.UtcNow;

        var incomingExperienceIds = experienceRows
            .Where(x => x.Id > 0)
            .Select(x => x.Id)
            .ToHashSet();
        var removedExperiences = profile.Experiences
            .Where(x => !incomingExperienceIds.Contains(x.Id))
            .ToList();
        _context.EmployeeOnboardingExperiences.RemoveRange(removedExperiences);

        var savedExperiences = new List<(SaveEmployeeOnboardingExperienceRequest Request, EmployeeOnboardingExperience Entity)>();
        foreach (var experience in experienceRows)
        {
            var experienceEntity = experience.Id > 0
                ? profile.Experiences.FirstOrDefault(x => x.Id == experience.Id)
                : null;

            if (experienceEntity == null)
            {
                experienceEntity = new EmployeeOnboardingExperience();
                profile.Experiences.Add(experienceEntity);
            }

            experienceEntity.CompanyName = experience.CompanyName;
            experienceEntity.JobTitle = experience.JobTitle;
            experienceEntity.YearsOfExperience = experience.YearsOfExperience;
            experienceEntity.RelievingEmailForwarded = experience.RelievingEmailForwarded;

            savedExperiences.Add((experience, experienceEntity));
        }

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var savedExperience in savedExperiences)
        {
            await SaveDocumentsAsync(
                request.EmployeeId,
                savedExperience.Request.ExperienceLetter,
                ExperienceLetterDocumentType,
                cancellationToken,
                savedExperience.Entity.Id);
        }

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
        CancellationToken cancellationToken,
        int? employeeOnboardingExperienceId = null)
    {
        if (file == null || file.Length == 0)
        {
            return;
        }

        var stored = await _fileStorageService.SaveAsync(file, employeeId, documentType, cancellationToken);
        await _context.EmployeeOnboardingDocuments.AddAsync(new EmployeeOnboardingDocument
        {
            EmployeeId = employeeId,
            EmployeeOnboardingExperienceId = employeeOnboardingExperienceId,
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

    public List<SaveEmployeeOnboardingExperienceRequest> Experiences { get; set; } = new();
}

public class SaveEmployeeOnboardingExperienceRequest
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? JobTitle { get; set; }

    public decimal? YearsOfExperience { get; set; }

    public bool RelievingEmailForwarded { get; set; }

    public IFormFile? ExperienceLetter { get; set; }
}
