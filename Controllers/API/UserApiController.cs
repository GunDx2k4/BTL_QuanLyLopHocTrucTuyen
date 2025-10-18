using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API
{
    [Route("api/user")]
    public class UserApiController(IUserRepository repository) : CrudApiController<User>(repository)
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return BadRequest($"User {User.Identity.Name} is already authenticated.");
            }

            var user = await repository.ValidateUser(request.Email, request.Password);
            if (user == null) return Unauthorized(new { message = "Invalid email or password" });

            var Permissions = user.Role != null ? user.Role.Permissions : Models.Enums.UserPermission.None;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("Permissions", ((int)Permissions).ToString())
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(new { User = user });
        }


        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }

        [UserPermissionAuthorize(UserPermission.ManagerUsers)]
        public override async Task<IActionResult> AddAsync([FromBody] User entity)
        {
            return await base.AddAsync(entity);
        }
    }
}
