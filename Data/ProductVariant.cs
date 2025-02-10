using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    [Table("ProductVariant")]
    public class ProductVariant
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? ProductId { get; set; }

        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product? Product { get; set; }
        [Range(0, double.MaxValue )]
        [Required]
        public decimal Price { get; set; }
        [Range(0, double.MaxValue)]
        public decimal DiscountPrice { get; set; }
        [Range(0, 1000)]
        [Required]
        public int Stock { get; set; }
        [MaxLength(1000)]
        [Required]
        public string Materials { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string SKU { get; set; } = string.Empty;
        public ICollection<ProductVariantValue> ProductVariantValues { get; set; } = new List<ProductVariantValue>();
    }
}