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
        public string? SelectedOptions { get; set; }
        // Read-only fields populated from Product navigation
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public decimal? BasePrice { get; set; }
    }
}