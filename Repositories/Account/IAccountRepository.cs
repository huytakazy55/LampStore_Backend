using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Models;
using System.Security.Claims;
using LampStoreProjects.Data;

namespace LampStoreProjects.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel model);
        Task<string?> SignInAsync(SignInModel model);
        Task<string?> GoogleSignInAsync(GoogleSignInModel model);

        Task<UserProfile?> GetUserProfileAsync(string userId);
        Task<IdentityUser?> GetUserAccountAsync(string userId);
        Task<IEnumerable<IdentityUser>> GetAllUsersAsync();
        Task<List<string>?> GetRolesByUserIdAsync(string userId);
        Task LogoutAsync(string userId);
        Task<string?> ForgotPasswordAsync(ForgotPasswordModel model);

    }
}