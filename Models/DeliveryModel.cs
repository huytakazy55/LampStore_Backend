using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class DeliveryModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryStatus { get; set; }

        // Order-backed fields used by the delivery management read model.
        public long OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderItemModel> OrderItems { get; set; } = new();
    }
}
