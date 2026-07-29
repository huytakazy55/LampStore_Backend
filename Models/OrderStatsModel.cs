namespace LampStoreProjects.Models
{
    public class OrderStatsModel
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Unpaid { get; set; }
        public int Shipping { get; set; }
        public int Completed { get; set; }
        public int FailedDelivery { get; set; }
        public int ReturnRequested { get; set; }
        public decimal Revenue { get; set; }
    }
}
