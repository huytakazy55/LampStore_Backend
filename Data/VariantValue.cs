using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    public class VariantValue
    {
        public Guid Id { get; set; }
        public Guid? TypeId { get; set; }
        [ForeignKey("TypeId")]
        [JsonIgnore]
        public VariantType? VariantType { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}