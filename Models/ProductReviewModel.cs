using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects
{
    public class ProductReviewModel
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal Rating { get; set; } = 0;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}