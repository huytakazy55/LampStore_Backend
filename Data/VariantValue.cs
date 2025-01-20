using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    public class VariantValue
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        [ForeignKey("TypeId")]
        [JsonIgnore]
        public VariantType? VariantType { get; set; }
        public string? Value { get; set; }
    }
}