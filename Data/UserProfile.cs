
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class UserProfile : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
        [MaxLength(20)]
        public string City { get; set; } = string.Empty;
        [MaxLength(100)]
        public string CityName { get; set; } = string.Empty;
        [MaxLength(20)]
        public string District { get; set; } = string.Empty;
        [MaxLength(100)]
        public string DistrictName { get; set; } = string.Empty;
        [MaxLength(20)]
        public string Ward { get; set; } = string.Empty;
        [MaxLength(100)]
        public string WardName { get; set; } = string.Empty;
        public string ProfileAvatar { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }      
    }
}
