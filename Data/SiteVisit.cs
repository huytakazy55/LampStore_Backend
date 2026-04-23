using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class SiteVisit : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string SessionId { get; set; } // Định danh phiên (cookie/localStorage)

        [MaxLength(50)]
        public string IpAddress { get; set; } // IP của Client để sau này map ra khu vực

        [MaxLength(255)]
        public string Path { get; set; } // URL/Page được truy cập

        public Guid? ProductId { get; set; } // Có giá trị nếu là trang chi tiết sản phẩm
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
    }
}
