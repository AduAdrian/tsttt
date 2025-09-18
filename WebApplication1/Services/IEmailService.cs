namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink);
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendEmailAsync(string email, string subject, string message);
    }
}