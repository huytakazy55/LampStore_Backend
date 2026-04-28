using System.Collections.Generic;
using LampStoreProjects.Helpers;

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
        public DateTime DateAdded { get; set; } = DateTimeHelper.VietnamNow;
        public bool Status {get; set; } = true;
        public Guid? AddOnProductId { get; set; }
        public ProductVariantDto ProductVariant { get; set; } = new ProductVariantDto(); 
        public List<VariantTypeDto> VariantTypes { get; set; } = new List<VariantTypeDto>();
    }

    public class VariantTypeUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public List<VariantValueItemDto> Values { get; set; } = new List<VariantValueItemDto>();
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