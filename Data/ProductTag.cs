namespace LampStoreProjects.Data
{
    public class ProductTag
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = new Product();
        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = new Tag();
    }

}
