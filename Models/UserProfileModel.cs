using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class UserProfileModel
    {
        [Key]
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfileAvatar { get; set; }
    }
}