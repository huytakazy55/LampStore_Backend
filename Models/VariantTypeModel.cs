using System.Text.Json.Serialization;

namespace LampStoreProjects.Models
{
    public class VariantTypeModel
    {
        public Guid Id {get; set;}
        public string Name {get; set;} = string.Empty;
        public Guid? ProductId {get; set;}
        [JsonIgnore]
        public ProductModel ProductModel {get; set;} = new ProductModel();
    }
}