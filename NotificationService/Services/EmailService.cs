using System.Net;
using System.Net.Mail;

namespace NotificationService.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Smtp:Host"];
                var smtpPort = _configuration["Smtp:Port"];
                var smtpUsername = _configuration["Smtp:Username"];
                var smtpPassword = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"];

                // Check if SMTP is configured
                if (string.IsNullOrEmpty(smtpHost) || smtpHost == "smtp.example.com")
                {
                    // Simulate sending email for development/demo
                    _logger.LogInformation("=== SIMULATED EMAIL SEND ===");
                    _logger.LogInformation("To: {to}", to);
                    _logger.LogInformation("Subject: {subject}", subject);
                    _logger.LogInformation("Body: {body}", body);
                    _logger.LogInformation("============================");
                    
                    // Simulate network delay
                    await Task.Delay(100);
                    return;
                }

                // Real SMTP sending
                using var client = new SmtpClient(smtpHost)
                {
                    Port = int.Parse(smtpPort ?? "587"),
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var message = new MailMessage(
                    from: fromEmail ?? "noreply@library.com",
                    to: to,
                    subject: subject,
                    body: body
                )
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {to}: {subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {to}", to);
                throw;
            }
        }
    }
}
