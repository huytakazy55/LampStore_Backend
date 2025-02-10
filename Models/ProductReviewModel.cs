using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects
{
    public class ProductReviewModel
    {
        public Guid Id { get; set;}
        public Guid? ProductId { get; set;}
        public ProductModel? ProductModel { get; set;}
        public string UserId { get; set;} = string.Empty;
        [Precision(18, 2)]
        public decimal Rating { get; set;} = 0;
        public string Comment { get; set;} = string.Empty;
        public DateTime CreateAt {get; set;} = DateTime.Now;
    }
}