using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    public class VariantType
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product? Product { get; set; }
        public ICollection<VariantValue> Values { get; set; } = new List<VariantValue>();
    }
}