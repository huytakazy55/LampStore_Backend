using System;

namespace LampStoreProjects.DTOs
{
    public class ProductImportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }
        public string Materials { get; set; } = string.Empty;
        public double? Weight { get; set; }
        public string SKU { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public bool Status { get; set; }
        public string Tags { get; set; } = string.Empty;
    }
} 