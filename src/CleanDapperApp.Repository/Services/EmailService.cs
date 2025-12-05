using System.Threading.Tasks;
using CleanDapperApp.Models.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanDapperApp.Repository.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Simulate sending email
            _logger.LogInformation($"Sending email to {to} with subject: {subject}");
            await Task.Delay(100); // Simulate network delay
            _logger.LogInformation("Email sent successfully.");
        }

        public async Task SendDailyReportsAsync()
        {
            _logger.LogInformation("Starting daily report generation...");
            // Logic to gather data and send reports
            await SendEmailAsync("admin@example.com", "Daily Report", "Here is your daily report...");
            _logger.LogInformation("Daily report sent.");
        }
    }
}
