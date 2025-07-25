using System.Text.Json.Serialization;

namespace LampStoreProjects.Models
{
    public class TagModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [JsonIgnore]
        public ICollection<ProductTagModel> ProductTags { get; set; } = new List<ProductTagModel>();
    }
}