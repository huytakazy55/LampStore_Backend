
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;
        public string ProfileAvatar { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }      
    }
}