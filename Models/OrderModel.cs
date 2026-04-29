using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Models
{
    public class OrderModel
    {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "Pending";
        public long OrderCode { get; set; }
        public string? CheckoutUrl { get; set; }

        // Shipping info
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? Note { get; set; }

        // Guest profile
        public string? GuestToken { get; set; }

        // Payment
        public string PaymentMethod { get; set; } = "cod";
        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }
        [Precision(18, 2)]
        public decimal ShippingFee { get; set; }

        // Order items
        public List<OrderItemModel>? OrderItems { get; set; }
    }

    public class OrderStatusUpdateModel
    {
        public string Status { get; set; } = string.Empty;
    }
}