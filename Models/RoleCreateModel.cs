using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class RoleCreateModel
    {
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;
    }
}

