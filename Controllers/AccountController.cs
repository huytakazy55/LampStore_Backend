using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;


namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;

        public AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, IConfiguration configuration, IAccountRepository accountRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _accountRepository = accountRepository;
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            try
            {
                var result = await _accountRepository.SignInAsync(signInModel);

                if (string.IsNullOrEmpty(result))
                {
                    return Unauthorized();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            try
            {
                var result = await _accountRepository.SignUpAsync(signUpModel);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "User registered successfully." });
                }

                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while signing up.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Console.WriteLine(userId);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var profile = new ProfileDto
            {
                Username = user.UserName,
                Email = user.Email,
            };

            return Ok(profile);
        }
    }
}