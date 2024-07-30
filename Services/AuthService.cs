using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Models;
using System.Threading.Tasks;

namespace LampStoreProjects.Services
{
    public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterModel model);
    Task<SignInResult> LoginAsync(LoginModel model);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterModel model)
    {
        var user = new ApplicationUser { UserName = model.Username, Email = model.Email, FullName = model.FullName };
        var result = await _userManager.CreateAsync(user, model.Password);
        return result;
    }

    public async Task<SignInResult> LoginAsync(LoginModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        return result;
    }
}

}