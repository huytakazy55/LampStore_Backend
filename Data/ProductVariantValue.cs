using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class ProductVariantValue : BaseEntity
    {
        public Guid? ProductVariantId { get; set; }
        [ForeignKey("ProductVariantId")]
        public Guid? VariantValueId { get; set; }
        [ForeignKey("VariantValueId")]
        public ProductVariant? ProductVariant { get; set; }
        public VariantValue? VariantValue { get; set; }
    }
}