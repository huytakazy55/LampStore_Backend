namespace LampStoreProjects.Models
{
    public class CartSyncItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? SelectedOptions { get; set; }
    }
}
