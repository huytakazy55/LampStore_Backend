namespace LampStoreProjects.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string username, string newPassword);
    }
} 