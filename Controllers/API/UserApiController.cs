using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API
{
    [Route("api/users")]
    public class UserApiController(IUserRepository userRepository, ITenantRepository tenantRepository) : CrudApiController<User>(userRepository)
    {
        [UserPermissionAuthorize(UserPermission.CreateUser | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> AddAsync([FromBody] User entity)
        {
            if (entity == null)
                return BadRequest(new { message = "User data is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var user = await userRepository.FindByIdAsync(userId);

            if (user == null || user.Role == null)
                return Forbid();

            var permissions = user.Role.Permissions;
            var isAdmin = permissions.HasPermission(UserPermission.ManageAllUsers);
            var isManager = permissions.HasPermission(UserPermission.CreateUser) && (user.TenantId != null || user.TenantId != Guid.Empty);
            if (isAdmin)
            {
                if (entity.TenantId == null)
                {
                    return BadRequest(new { message = "TenantId is required." });
                }

                return await base.AddAsync(entity);
            }

            if (!isManager)
                return Forbid();
            if (entity.TenantId == null || entity.TenantId == Guid.Empty) entity.TenantId = user.TenantId;

            var existingUser = await userRepository.FindByEmailAsync(entity.Email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already in use." });
            }

            return await base.AddAsync(entity);
        }

        [UserPermissionAuthorize(UserPermission.ViewUsers | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> GetAsync()
        {
            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var user = await userRepository.FindByIdAsync(userId);
            if (user != null && user.Role != null && user.Role.Permissions.HasPermission(UserPermission.ViewUsers))
            {
                var result = (await userRepository.FindAsync()).Where(u => u.TenantId == user.TenantId);
                return Ok(result);
            }

            return await base.GetAsync();
        }

        [UserPermissionAuthorize(UserPermission.EditUser | UserPermission.EditUsers | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> UpdateAsync([FromBody] User entity)
        {
            if (entity == null)
                return BadRequest(new { message = "User data is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            if (userId == entity.Id)
            {
                return await base.UpdateAsync(entity);
            }

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllUsers);
            var isManager = permissions.Value.HasPermission(UserPermission.EditUsers) && await userRepository.IsSameTenantAsync(userId, entity.Id);

            if (!isAdmin || !isManager) return Forbid();

            var existingUser = await userRepository.FindByEmailAsync(entity.Email);

            if (existingUser != null && existingUser.Id != entity.Id)
            {
                return BadRequest(new { message = "Email is already in use." });
            }

            return await base.UpdateAsync(entity);
        }

        [UserPermissionAuthorize(UserPermission.ViewUser | UserPermission.ViewUsers | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Valid user ID is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            if (userId == id)
            {
                return await base.GetByIdAsync(id);
            }

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllUsers);
            var isManager = permissions.Value.HasPermission(UserPermission.ViewUsers) && await userRepository.IsSameTenantAsync(userId, id);
            if (!isAdmin && !isManager) return Forbid();

            return await base.GetByIdAsync(id);
        }

        [UserPermissionAuthorize(UserPermission.DeleteUser | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> DeleteByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Valid user ID is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllUsers);
            var isManager = permissions.Value.HasPermission(UserPermission.DeleteUser) && await userRepository.IsSameTenantAsync(userId, id);
            if (!isAdmin && !isManager) return Forbid();
            return await base.DeleteByIdAsync(id);
        }

        [UserPermissionAuthorize(UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> DeleteAllAsync()
        {
            return await base.DeleteAllAsync();
        }

        [HttpGet("instructors")]
        [UserPermissionAuthorize(UserPermission.ViewUsers | UserPermission.ManageAllUsers)]
        public async Task<IActionResult> GetInstructorsAsync()
        {
            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var user = await userRepository.FindByIdAsync(userId);
            if (user != null && user.Role != null && user.Role.Permissions.HasPermission(UserPermission.ViewUsers))
            {
                var result = (await userRepository.FindAsync()).Where(u => u.TenantId == user.TenantId && u.Role != null && u.Role.Permissions.HasPermission(UserPermission.InstructorUser));
                return Ok(result);
            }

            var result2 = (await userRepository.FindAsync()).Where(u => u.Role != null && u.Role.Permissions.HasPermission(UserPermission.InstructorUser));
            return Ok(result2);
        }

        [HttpPost("instructors")]
        [UserPermissionAuthorize(UserPermission.CreateUser | UserPermission.ManageAllUsers)]
        public async Task<IActionResult> AddInstructor([FromBody] CreateUserViewModel entity)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
                return Forbid();
            var user = await userRepository.FindByIdAsync(userId);
            if (user == null)
                return Forbid();

            if (user.TenantId == null || user.TenantId == Guid.Empty)
                return BadRequest(new { message = "User's tenant information is invalid." });

            var role = await tenantRepository.GetRoleInstructorDefaultAsync(user.TenantId!.Value);
            if (role == null)
                return BadRequest(new { message = "Default instructor role not found." });

            var newUser = new User
            {
                FullName = entity.FullName,
                Email = entity.Email,
                PasswordHash = SecurityHelper.HashPassword(entity.Password),
                RoleId = role.Id,
                TenantId = user.TenantId
            };
            return await AddAsync(newUser);
        }

        [HttpPost("students")]
        [UserPermissionAuthorize(UserPermission.CreateUser | UserPermission.ManageAllUsers)]
        public async Task<IActionResult> AddStudent([FromBody] CreateUserViewModel entity)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
                return Forbid();
            var user = await userRepository.FindByIdAsync(userId);
            if (user == null)
                return Forbid();
                
            if (user.TenantId == null || user.TenantId == Guid.Empty)
                return BadRequest(new { message = "User's tenant information is invalid." });

            var role = await tenantRepository.GetRoleStudentDefaultAsync(user.TenantId!.Value);
            if (role == null)
                return BadRequest(new { message = "Default student role not found." });

            var newUser = new User
            {
                FullName = entity.FullName,
                Email = entity.Email,
                PasswordHash = SecurityHelper.HashPassword(entity.Password),
                RoleId = role.Id,
                TenantId = user.TenantId
            };
            return await AddAsync(newUser);
        }
    }
}
