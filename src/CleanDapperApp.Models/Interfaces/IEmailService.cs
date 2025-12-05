using System.Threading.Tasks;

namespace CleanDapperApp.Models.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendDailyReportsAsync();
    }
}
