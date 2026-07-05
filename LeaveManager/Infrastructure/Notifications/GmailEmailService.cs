using System.Net;
using System.Net.Mail;

namespace LeaveManager.Infrastructure.Notifications;

public class GmailEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public GmailEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpClient = new SmtpClient(_config["Smtp:Host"])
        {
            Port = int.Parse(_config["Smtp:Port"]!),
            Credentials = new NetworkCredential(
                _config["Smtp:Username"],
                _config["Smtp:Password"]
            ),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"]!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        await smtpClient.SendMailAsync(mail);
    }
}