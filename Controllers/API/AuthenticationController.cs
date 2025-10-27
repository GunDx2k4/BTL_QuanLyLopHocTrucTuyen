using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API
{
    [Route("api/authen")]
    [ApiController]
    public class AuthenticationController(IUserRepository repository, IMemoryCache memoryCache) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await repository.ValidateUser(request.Email, request.Password);
            if (user == null) return Unauthorized(new { message = "Invalid email or password" });

            if (memoryCache.TryGetValue(user.Id, out Guid cacheSession))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return BadRequest("User is already logged in from another session.");
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return BadRequest($"User {User.Identity.Name} is already authenticated.");
            }

            var sessionId = Guid.NewGuid();
            memoryCache.Set(user.Id, sessionId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("SessionId", sessionId.ToString())
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            return Ok(new { message = $"Login {user.FullName} successful" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            if (userId != Guid.Empty)
            {
                memoryCache.Remove(userId);
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var existingUser = await repository.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already in use." });
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = SecurityHelper.HashPassword(request.Password)
            };

            await repository.AddAsync(user);

            return Ok(new { message = "User registered successfully" });
        }
    }
}
