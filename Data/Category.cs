using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class Category : BaseEntity
    {

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsDisplayed { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}