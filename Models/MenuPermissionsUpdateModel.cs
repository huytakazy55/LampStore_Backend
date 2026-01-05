using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class MenuPermissionsUpdateModel
    {
        [Required]
        public List<string> Menus { get; set; } = new();
    }
}

