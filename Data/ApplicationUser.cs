using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Data;

public class ApplicationUser : IdentityUser
{
    public string? GoogleUserId { get; set; } // Google OAuth User ID
    public ICollection<Cart>? Carts { get; set; }
    public ICollection<CheckIn>? CheckIns { get; set; }
    public ICollection<Order>? Orders { get; set; }
}