
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        [MaxLength(100)]
        [Required]
        public string? FullName { get; set; }
        [MaxLength(100)]
        [Required]
        public string? Email { get; set; }
        [MaxLength(20)]
        [Required]
        public string? PhoneNumber { get; set; }
        [MaxLength(100)]
        [Required]
        public string? Address { get; set; }
        public string? ProfileAvatar { get; set; }
        public ApplicationUser? User { get; set; }
    }
}