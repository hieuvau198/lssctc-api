namespace Lssctc.ProgramManagement.Accounts.Authens.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
