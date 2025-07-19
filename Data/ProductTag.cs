namespace LampStoreProjects.Data
{
    public class ProductTag : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = new Product();
        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = new Tag();
    }

}
