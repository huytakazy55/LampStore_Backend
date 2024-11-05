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
        [Required]
        [MaxLength(30)]
        public string? Type { get; set; }
        [Required]
        [MaxLength(30)]
        public string? Value { get; set; }
    }
}