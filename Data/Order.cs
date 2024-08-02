using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
        public ICollection<Delivery>? Deliveries { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}