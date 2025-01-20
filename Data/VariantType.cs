using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    public class VariantType
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ProductVariantId { get; set; }
        [ForeignKey("ProductVariantId")]
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }
        public ICollection<VariantValue> Values { get; set; } = new List<VariantValue>();
    }
}