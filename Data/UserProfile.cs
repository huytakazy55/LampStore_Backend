
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
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
        public string? Andress { get; set; }
        public string? ProfileAvatar { get; set; }
        public ApplicationUser? User { get; set; }
    }
}