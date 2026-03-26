using EmailService.Application.Common.Interfaces;
using EmailService.Domain.Entities;

namespace EmailService.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(Email email)
        {
            _logger.LogInformation("📧 Sending email to {Email}", email.To);

            // simulate sending
            _logger.LogInformation("✅ Email sent to {Email}", email.To);

            return Task.CompletedTask;
        }
    }
}
