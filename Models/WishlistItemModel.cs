namespace LampStoreProjects.Models
{
    public class WishlistItemModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? CategoryName { get; set; }
        public int SellCount { get; set; }
        public bool ProductStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
