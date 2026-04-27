namespace LampStoreProjects.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string username, string newPassword);
        Task<bool> SendOrderConfirmationEmailAsync(Models.OrderModel order, string storeUrl = "https://capylumine.com");
        Task<bool> SendNewOrderNotificationToAdminAsync(Models.OrderModel order, IEnumerable<string> adminEmails, string storeUrl = "https://capylumine.com");
    }
} 