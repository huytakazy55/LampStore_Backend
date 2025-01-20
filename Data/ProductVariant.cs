using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    [Table("ProductVariant")]
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product? Product { get; set; }
        [Range(0, double.MaxValue)]
        [Required]
        public double OriginalPrice { get; set; }
        [Range(0, 100)]
        public double? Discount { get; set; }
        [Range(0, double.MaxValue)]
        public double SalePrice { get; set; }
        [Range(0, 1000)]
        [Required]
        public int Quantity { get; set; }
        [MaxLength(1000)]
        [Required]
        public string? Materials { get; set; }
        public double? Weight { get; set; }
        public bool IsAvailable { get; set; } = true;
        public ICollection<VariantType> Types { get; set; } = new List<VariantType>();
    }
}