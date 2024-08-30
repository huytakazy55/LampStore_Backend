using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Models;
using System.Security.Claims;
using LampStoreProjects.Data;

namespace LampStoreProjects.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel model);
        Task<string> SignInAsync(SignInModel model);

        Task<UserProfile> GetUserProfileAsync(string userId);
        Task<IdentityUser> GetUserAccountAsync(string userId);

        Task LogoutAsync(string userId);

    }
}