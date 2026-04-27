using System.ComponentModel.DataAnnotations;
using LampStoreProjects.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Models
{
    public class FlashSaleModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTimeHelper.VietnamNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<FlashSaleItemModel> Items { get; set; } = new List<FlashSaleItemModel>();
    }

    public class FlashSaleItemModel
    {
        [Key]
        public int Id { get; set; }
        
        public int FlashSaleId { get; set; }
        
        public Guid ProductId { get; set; }
        
        public int DiscountPercent { get; set; }
        
        [Precision(18, 2)]
        public decimal FlashSalePrice { get; set; }
        
        public int Stock { get; set; }
        
        public int SoldCount { get; set; } = 0;
        
        public int Order { get; set; } = 0;

        // Navigation data (populated in repository)
        public string? ProductName { get; set; }
        public string? ProductSlug { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}
