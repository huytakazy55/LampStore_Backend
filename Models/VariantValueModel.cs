using System.Text.Json.Serialization;

namespace LampStoreProjects.Models
{
    public class VariantValueModel
    {
        public Guid Id {get; set;}
        public Guid? TypeId {get; set;}
        [JsonIgnore]
        public VariantTypeModel VariantTypeModel {get; set;} = new VariantTypeModel();
        public string Value {get; set;} = string.Empty;
    }

}