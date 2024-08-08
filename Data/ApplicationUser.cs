using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Data;

public class ApplicationUser : IdentityUser
{
    public ICollection<Cart>? Carts { get; set; }
    public ICollection<CheckIn>? CheckIns { get; set; }
    public ICollection<Order>? Orders { get; set; }
}