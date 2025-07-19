using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    public class VariantValue : BaseEntity
    {
        public Guid? TypeId { get; set; }
        [ForeignKey("TypeId")]
        [JsonIgnore]
        public VariantType? VariantType { get; set; }
        public string Value { get; set; } = string.Empty;
        public ICollection<ProductVariantValue> ProductVariantValues { get; set; } = new List<ProductVariantValue>();
    }
}