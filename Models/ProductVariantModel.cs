using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class ProductVariantModel
    {
        public int Id { get; set;}
        public int ProductId { get; set;}
        public ProductModel? ProductModel{ get; set;}
        public string? Materials { get; set;}
        public int Quantity {get; set;}
        public double OriginalPrice {get; set;}
        public double Discount {get; set;}
        public double SalePrice {get; set;}
        public double Weight {get; set;}
        public bool IsAvailable {get; set;}
    }
}