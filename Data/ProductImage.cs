using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    [Table("ProductImage")]
    public class ProductImage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ImagePath { get; set; } = string.Empty;

        public Guid? ProductId { get; set; }

        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}