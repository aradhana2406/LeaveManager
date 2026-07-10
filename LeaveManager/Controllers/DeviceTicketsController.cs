using LeaveManager.Common.Enums;
using LeaveManager.Data;
using LeaveManager.Entities;
using LeaveManager.Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Controllers;

[ApiController]
[Route("api/device-tickets")]
public class DeviceTicketsController : ControllerBase
{
    private const string SubmittedStatus = "Submitted";
    private const string HrReviewStatus = "HR Review";
    private const string CancelledStatus = "Cancelled";
    private const string DefaultNotificationTo = "devicehelp@company.com";
    private const string DefaultNotificationCc = "hr@company.com";

    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<DeviceTicketsController> _logger;

    public DeviceTicketsController(
        AppDbContext context,
        IEmailService emailService,
        ILogger<DeviceTicketsController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetEmployeeTickets(int employeeId, CancellationToken cancellationToken)
    {
        var tickets = await TicketQuery()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync(cancellationToken);

        return Ok(new { Tickets = tickets.Select(ToTicketDto) });
    }

    [HttpGet("hr")]
    public async Task<IActionResult> GetHrTickets(CancellationToken cancellationToken)
    {
        var tickets = await TicketQuery()
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync(cancellationToken);

        return Ok(new { Tickets = tickets.Select(ToTicketDto) });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(CreateDeviceTicketRequest request, CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0 ||
            string.IsNullOrWhiteSpace(request.RequestType) ||
            string.IsNullOrWhiteSpace(request.DeviceType) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { Message = "Employee, request type, device type, subject, and details are required." });
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId && x.IsActive, cancellationToken);

        if (employee == null)
        {
            return BadRequest(new { Message = "Employee not found." });
        }

        var now = DateTime.UtcNow;
        var ticket = new EmployeeDeviceTicket
        {
            EmployeeId = employee.Id,
            RequestType = request.RequestType.Trim(),
            DeviceType = request.DeviceType.Trim(),
            NotificationTo = string.IsNullOrWhiteSpace(request.NotificationTo)
                ? DefaultNotificationTo
                : request.NotificationTo.Trim(),
            NotificationCc = string.IsNullOrWhiteSpace(request.NotificationCc)
                ? DefaultNotificationCc
                : request.NotificationCc.Trim(),
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            Status = SubmittedStatus,
            CreatedOn = now,
            LastUpdatedOn = now
        };

        ticket.TimelineEvents.Add(new EmployeeDeviceTicketTimelineEvent
        {
            Status = SubmittedStatus,
            Notes = "Employee submitted the device request.",
            CreatedOn = now
        });
        ticket.TimelineEvents.Add(new EmployeeDeviceTicketTimelineEvent
        {
            Status = HrReviewStatus,
            Notes = "Request is available in the HR device queue. Any active HR member can review and coordinate next steps.",
            CreatedOn = now.AddSeconds(1)
        });

        await _context.EmployeeDeviceTickets.AddAsync(ticket, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var emailSent = await TryNotifyHrAsync(employee, ticket, cancellationToken);

        return Ok(new
        {
            Message = emailSent
                ? "Device ticket generated and HR has been notified."
                : "Device ticket generated. HR email notification could not be sent.",
            TicketId = ticket.Id,
            EmailSent = emailSent
        });
    }

    [HttpPost("{ticketId:int}/timeline")]
    public async Task<IActionResult> AddTimelineEvent(
        int ticketId,
        AddDeviceTicketTimelineRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status) || string.IsNullOrWhiteSpace(request.Notes))
        {
            return BadRequest(new { Message = "Status and notes are required." });
        }

        var ticket = await _context.EmployeeDeviceTickets
            .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);

        if (ticket == null)
        {
            return NotFound(new { Message = "Ticket was not found." });
        }

        if (request.AssignedHrId.HasValue)
        {
            var hrExists = await _context.Employees.AnyAsync(
                x => x.Id == request.AssignedHrId.Value &&
                     x.IsActive &&
                     (x.Role == Role.HR || x.Role == Role.HRL2),
                cancellationToken);

            if (!hrExists)
            {
                return BadRequest(new { Message = "Selected HR employee was not found." });
            }

            ticket.AssignedHrId = request.AssignedHrId.Value;
        }

        var now = DateTime.UtcNow;
        ticket.Status = request.Status.Trim();
        ticket.LastUpdatedOn = now;

        await _context.EmployeeDeviceTicketTimelineEvents.AddAsync(new EmployeeDeviceTicketTimelineEvent
        {
            EmployeeDeviceTicketId = ticket.Id,
            Status = ticket.Status,
            Notes = request.Notes.Trim(),
            CreatedOn = now
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Ticket timeline updated." });
    }

    [HttpPost("{ticketId:int}/cancel")]
    public async Task<IActionResult> CancelTicket(
        int ticketId,
        CancelDeviceTicketRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0)
        {
            return BadRequest(new { Message = "Employee is required." });
        }

        var ticket = await _context.EmployeeDeviceTickets
            .FirstOrDefaultAsync(
                x => x.Id == ticketId && x.EmployeeId == request.EmployeeId,
                cancellationToken);

        if (ticket == null)
        {
            return NotFound(new { Message = "Ticket was not found for this employee." });
        }

        if (IsClosedStatus(ticket.Status))
        {
            return BadRequest(new { Message = "This ticket is already closed and cannot be cancelled." });
        }

        var now = DateTime.UtcNow;
        ticket.Status = CancelledStatus;
        ticket.LastUpdatedOn = now;

        var note = string.IsNullOrWhiteSpace(request.Reason)
            ? "Employee cancelled the device request."
            : "Employee cancelled the device request: " + request.Reason.Trim();

        await _context.EmployeeDeviceTicketTimelineEvents.AddAsync(new EmployeeDeviceTicketTimelineEvent
        {
            EmployeeDeviceTicketId = ticket.Id,
            Status = CancelledStatus,
            Notes = note,
            CreatedOn = now
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Device ticket cancelled." });
    }

    private IQueryable<EmployeeDeviceTicket> TicketQuery()
    {
        return _context.EmployeeDeviceTickets
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.AssignedHr)
            .Include(x => x.TimelineEvents);
    }

    private static object ToTicketDto(EmployeeDeviceTicket ticket)
    {
        return new
        {
            ticket.Id,
            ticket.EmployeeId,
            EmployeeName = ticket.Employee.FullName,
            EmployeeCode = ticket.Employee.EmployeeCode,
            EmployeeEmail = ticket.Employee.Email,
            ticket.AssignedHrId,
            AssignedHrName = ticket.AssignedHr == null ? null : ticket.AssignedHr.FullName,
            ticket.RequestType,
            ticket.DeviceType,
            ticket.NotificationTo,
            ticket.NotificationCc,
            ticket.Subject,
            ticket.Description,
            ticket.Status,
            ticket.CreatedOn,
            ticket.LastUpdatedOn,
            Timeline = ticket.TimelineEvents
                .OrderBy(x => x.CreatedOn)
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    x.Notes,
                    x.CreatedOn
                })
                .ToList()
        };
    }

    private static bool IsClosedStatus(string? status)
    {
        return string.Equals(status, CancelledStatus, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Resolved", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Closed", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> TryNotifyHrAsync(
        Employee employee,
        EmployeeDeviceTicket ticket,
        CancellationToken cancellationToken)
    {
        var body = GenerateHrNotificationEmail(employee, ticket);
        try
        {
            await _emailService.SendEmailAsync(
                string.IsNullOrWhiteSpace(ticket.NotificationTo) ? DefaultNotificationTo : ticket.NotificationTo,
                ticket.Subject,
                body,
                string.IsNullOrWhiteSpace(ticket.NotificationCc) ? DefaultNotificationCc : ticket.NotificationCc,
                cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send device ticket notification {TicketId}", ticket.Id);
            return false;
        }
    }

    private static string GenerateHrNotificationEmail(Employee employee, EmployeeDeviceTicket ticket)
    {
        return $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; color: #24302b;'>
    <div style='max-width: 680px; margin: 0 auto; padding: 24px;'>
        <h2 style='margin-bottom: 12px;'>New device request #{ticket.Id}</h2>
        <p>{employee.FullName} ({employee.EmployeeCode}) submitted a {ticket.RequestType.ToLowerInvariant()} request for {ticket.DeviceType}.</p>
        <div style='padding: 16px; border: 1px solid #ded7c8; border-radius: 8px; background: #fffdf8; margin: 18px 0;'>
            <p style='margin: 0 0 8px;'><strong>To:</strong> {ticket.NotificationTo}</p>
            <p style='margin: 0 0 8px;'><strong>Cc:</strong> {ticket.NotificationCc}</p>
            <p style='margin: 0 0 8px;'><strong>Subject:</strong> {ticket.Subject}</p>
            <p style='margin: 0 0 8px;'><strong>Details:</strong> {ticket.Description}</p>
            <p style='margin: 0;'><strong>Timeline:</strong> Submitted -> HR Review -> Coordination -> Resolved</p>
        </div>
        <p>Please open LeaveManager and review the HR device queue.</p>
    </div>
</body>
</html>";
    }
}

public class CreateDeviceTicketRequest
{
    public int EmployeeId { get; set; }

    public string RequestType { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;

    public string NotificationTo { get; set; } = "devicehelp@company.com";

    public string NotificationCc { get; set; } = "hr@company.com";

    public string Subject { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public class AddDeviceTicketTimelineRequest
{
    public string Status { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public int? AssignedHrId { get; set; }
}

public class CancelDeviceTicketRequest
{
    public int EmployeeId { get; set; }

    public string? Reason { get; set; }
}
