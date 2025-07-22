using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class Message : BaseEntity
    {
        [Required]
        public Guid ChatId { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; } = MessageType.Text;

        public string? AttachmentUrl { get; set; } // Đường dẫn file đính kèm

        public bool IsRead { get; set; } = false;

        public DateTime ReadAt { get; set; }

        // Navigation properties
        public Chat Chat { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;
    }

    public enum MessageType
    {
        Text = 1,
        Image = 2,
        File = 3,
        System = 4  // Tin nhắn hệ thống (VD: "Admin đã tham gia chat")
    }
} 