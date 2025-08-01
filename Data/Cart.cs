using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class Cart : BaseEntity
    {
        public string? UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
    }
}