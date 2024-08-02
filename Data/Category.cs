using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class Category
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}