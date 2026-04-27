using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Data
{
    public class FlashSale : ITimestampEntity
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
        
        public ICollection<FlashSaleItem> Items { get; set; } = new List<FlashSaleItem>();
    }

    public class FlashSaleItem
    {
        [Key]
        public int Id { get; set; }
        
        public int FlashSaleId { get; set; }
        
        [ForeignKey("FlashSaleId")]
        public FlashSale? FlashSale { get; set; }
        
        public Guid ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        
        /// <summary>Phần trăm giảm giá (1-90)</summary>
        public int DiscountPercent { get; set; }
        
        /// <summary>Giá flash sale (tính từ MinPrice * (100-DiscountPercent)/100)</summary>
        [Precision(18, 2)]
        public decimal FlashSalePrice { get; set; }
        
        /// <summary>Số lượng dành cho flash sale</summary>
        public int Stock { get; set; }
        
        /// <summary>Số lượng đã bán trong flash sale</summary>
        public int SoldCount { get; set; } = 0;
        
        /// <summary>Thứ tự hiển thị</summary>
        public int Order { get; set; } = 0;
    }
}
