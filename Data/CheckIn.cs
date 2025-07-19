using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class CheckIn : BaseEntity
    {
        public string? UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        public DateTime CheckInDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}