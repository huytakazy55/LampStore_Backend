using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class ProductVariantModel
    {
        public int Id { get; set;}
        public int ProductId { get; set;}
        public ProductModel? ProductModel{ get; set;}
        [Required]
        public string? Type { get; set;}
        [Required]
        public string? Value { get; set;}
    }

    public class ProductVariantCreateModel
    {
            [Required]
            public string? Type { get; set; }

            [Required]
            public string? Value { get; set; }
    }
    
}