using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Models
{
    public class CartItemModel
    {
        public Guid Id { get; set; }
        public Guid? CartId { get; set; }
        public Guid? ProductId { get; set; }
        public int Quantity { get; set; }
    }
}