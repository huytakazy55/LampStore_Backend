using System.ComponentModel.DataAnnotations;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Data
{
    public class Chat : BaseEntity
    {
        public string? UserId { get; set; }

        // Guest chat support
        public string? GuestToken { get; set; }
        public string? GuestName { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty; // Tiêu đề yêu cầu hỗ trợ

        public ChatStatus Status { get; set; } = ChatStatus.Open;

        public ChatPriority Priority { get; set; } = ChatPriority.Normal;

        public string? AssignedAdminId { get; set; } // Admin được assign

        public DateTime LastMessageAt { get; set; } = DateTimeHelper.VietnamNow;

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