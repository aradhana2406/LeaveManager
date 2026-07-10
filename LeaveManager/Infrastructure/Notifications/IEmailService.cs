namespace LeaveManager.Infrastructure.Notifications;

public interface IEmailService
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        string? cc = null,
        CancellationToken cancellationToken = default);
}
