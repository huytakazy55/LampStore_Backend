using System.Collections.Generic;

namespace LampStoreProjects.DTOs
{
    public class ProductUpdateDto 
    {
        public Guid Id { get; set;}
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReviewCount { get; set; } = 0;
        public string Tags { get; set; } = string.Empty;
        public int ViewCount { get; set; } = 0;
        public int Favorites { get; set; } = 0;
        public int SellCount { get; set; } = 0;
        public Guid? CategoryId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public int Status {get; set; } = 1;
        public List<ProductVariantDto> ProductVariants { get; set; } = new List<ProductVariantDto>(); 
        public List<VariantTypeDto> VariantTypes { get; set; } = new List<VariantTypeDto>();
    }

    public class VariantTypeUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new List<string>();
    }

    public class ProductVariantUpdateDto
    {
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public int Stock { get; set; }
        public double Weight { get; set; }
        public string Materials { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
    }
}