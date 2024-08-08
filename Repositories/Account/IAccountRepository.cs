using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Models;
using System.Security.Claims;

namespace LampStoreProjects.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel model);
        Task<string> SignInAsync(SignInModel model);

        // Task<UserProfileModel> GetUserProfileAsync(ClaimsPrincipal userPrincipal);

        Task LogoutAsync(ClaimsPrincipal userPrincipal);
    }
}