using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Models
{
    public class ProductVariantModel
    {
        public Guid Id { get; set;}
        public Guid? ProductId { get; set;}
        public ProductModel ProductModel{ get; set;} = new ProductModel();
        [Precision(18, 2)]
        public decimal Price {get; set;}
        [Precision(18, 2)]
        public decimal DiscountPrice {get; set;}
        public int Stock {get; set;} 
        public double Weight {get; set;}
        public string Materials { get; set;} = string.Empty;
        public string SKU {get; set;} = string.Empty;
    }
}