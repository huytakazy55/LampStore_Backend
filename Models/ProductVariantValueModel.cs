namespace LampStoreProjects.Models
{
    public class ProductVariantValueModel
    {
        public Guid Id { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? VariantValueId { get; set; }

        public ProductVariantModel? ProductVariantModel { get; set; } 
        public VariantValueModel? VariantValueModel { get; set; } 
    }
}