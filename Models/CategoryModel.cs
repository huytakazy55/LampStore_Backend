using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class CategoryModel
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsDisplayed { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
    }
}