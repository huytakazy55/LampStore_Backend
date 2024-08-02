using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class CartModel
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public string? UserId { get; set; }
    }
}