using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Models
{
    public class ProductStatsModel
    {
        public int ReviewCount { get; set; }

        [Precision(5, 2)]
        public decimal AverageRating { get; set; }

        public int SellCount { get; set; }

        public int Stock { get; set; }
    }
}
