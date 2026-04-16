using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Models
{
    public class OrderItemModel
    {
        public Guid? Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public string? SelectedOptions { get; set; }
    }
}