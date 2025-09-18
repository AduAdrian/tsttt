namespace WebApplication1.Services
{
    public interface ISmsService
    {
        Task SendPasswordResetSmsAsync(string phoneNumber, string resetCode);
        Task SendSmsAsync(string phoneNumber, string message);
    }
}