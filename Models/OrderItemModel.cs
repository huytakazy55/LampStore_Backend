using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class OrderItemModel
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}