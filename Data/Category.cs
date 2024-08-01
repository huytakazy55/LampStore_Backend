using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LampStoreProjects.Models;

namespace LampStoreProjects.Data
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public ICollection<LampModel> Lamps { get; set; } = new List<LampModel>();
    }
}