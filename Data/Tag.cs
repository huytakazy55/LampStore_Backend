namespace LampStoreProjects.Data
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }

}
