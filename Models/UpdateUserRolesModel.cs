using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class UpdateUserRolesModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "Cần ít nhất 1 quyền.")]
        public List<string> Roles { get; set; } = new();
    }
}


