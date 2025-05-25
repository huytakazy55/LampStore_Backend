namespace LampStoreProjects.Models
{
    public class ProductTagModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public ProductModel Product { get; set; } = new ProductModel();
        public Guid TagId { get; set; }
        public TagModel Tag { get; set; } = new TagModel();
    }
}