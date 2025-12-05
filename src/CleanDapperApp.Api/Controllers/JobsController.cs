using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace CleanDapperApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;

        public JobsController(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        [HttpPost("enqueue-email")]
        public IActionResult EnqueueEmail(string to, string subject, string body)
        {
            _backgroundJobClient.Enqueue<IEmailService>(x => x.SendEmailAsync(to, subject, body));
            return Ok(new { Message = "Email job enqueued!" });
        }

        [HttpPost("schedule-daily-report")]
        public IActionResult ScheduleDailyReport()
        {
            // Schedule to run at 12:00 PM (noon) daily
            _recurringJobManager.AddOrUpdate<IEmailService>(
                "daily-report-job",
                x => x.SendDailyReportsAsync(),
                Cron.Daily(12, 0));

            return Ok(new { Message = "Daily report job scheduled for 12:00 PM every day!" });
        }
    }
}
