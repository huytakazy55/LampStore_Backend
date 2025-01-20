using System.Text.Json.Serialization;

namespace LampStoreProjects.Models
{
    public class VariantValueModel
    {
        public int Id {get; set;}
        public int TypeId {get; set;}
        [JsonIgnore]
        public VariantTypeModel? VariantTypeModel {get; set;}
        public string? Value {get; set;}
    }
}