using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class UserProfileModel
    {
        [Key]
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string ProfileAvatar { get; set; } = string.Empty;
    }
}
