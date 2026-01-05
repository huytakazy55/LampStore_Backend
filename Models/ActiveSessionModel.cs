namespace LampStoreProjects.Models
{
    /// <summary>
    /// Model hiển thị session đang active
    /// </summary>
    public class ActiveSessionModel
    {
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public bool IsCurrentSession { get; set; }
    }
}

