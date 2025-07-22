using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class Chat : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty; // Tiêu đề yêu cầu hỗ trợ

        public ChatStatus Status { get; set; } = ChatStatus.Open;

        public ChatPriority Priority { get; set; } = ChatPriority.Normal;

        public string? AssignedAdminId { get; set; } // Admin được assign

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public ApplicationUser? AssignedAdmin { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public enum ChatStatus
    {
        Open = 1,      // Mở, chờ phản hồi
        InProgress = 2, // Đang xử lý
        Resolved = 3,   // Đã giải quyết
        Closed = 4      // Đã đóng
    }

    public enum ChatPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }
} 