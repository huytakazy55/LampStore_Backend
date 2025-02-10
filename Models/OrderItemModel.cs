using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Models
{
    public class OrderItemModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
    }
}