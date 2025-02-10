using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Models
{
    public class CheckInModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}