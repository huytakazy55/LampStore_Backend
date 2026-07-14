using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Data
{
    public class DiscountCode : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "FixedAmount"

        [Precision(18, 2)]
        public decimal DiscountPercentage { get; set; }

        [Precision(18, 2)]
        public decimal DiscountAmount { get; set; } // For FixedAmount type

        [Precision(18, 2)]
        public decimal MaxDiscountAmount { get; set; }

        [Precision(18, 2)]
        public decimal MinOrderAmount { get; set; }

        public int Quantity { get; set; } = 1;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // "Active", "Inactive"

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public string? UserId { get; set; } // Nullable for global codes

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
