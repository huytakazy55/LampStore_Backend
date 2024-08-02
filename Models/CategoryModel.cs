using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}