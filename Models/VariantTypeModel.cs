using System.Text.Json.Serialization;

namespace LampStoreProjects.Models
{
    public class VariantTypeModel
    {
        public int Id {get; set;}
        public string? Name {get; set;}
        [JsonIgnore]
        public ProductVariantModel? ProductVariantModel {get; set;}
        public int ProductVariantId {get; set;}
    }
}