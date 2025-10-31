using System;
using System.Linq;
using System.Threading.Tasks;
using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API;

[Route("api/roles")]
public class RoleApiController(IRoleRepository roleRepository, IUserRepository userRepository) : CrudApiController<Role>(roleRepository)
{
    [UserPermissionAuthorize(UserPermission.CreateRole | UserPermission.ManageAllRoles)]
    public override async Task<IActionResult> AddAsync([FromBody] Role entity)
    {
        if (entity == null)
            return BadRequest(new { message = "Role data is required." });

        var userId = User.GetUserId();

        if (userId == Guid.Empty)
            return Forbid();

        var user = await userRepository.FindByIdAsync(userId);

        if (user == null || user.Role == null)
            return Forbid();

        var permissions = user.Role.Permissions;
        var isAdmin = permissions.HasPermission(UserPermission.ManageAllRoles);
        var isManager = permissions.HasPermission(UserPermission.CreateRole) && (user.TenantId != null && user.TenantId != Guid.Empty);

        if (isAdmin)
        {
            if (entity.TenantId == null)
            {
                return BadRequest(new { message = "TenantId is required." });
            }

            // ensure unique name within tenant
            if (await roleRepository.RoleNameExistsAsync(entity.Name, entity.TenantId!.Value))
                return BadRequest(new { message = "Role name is already in use." });

            return await base.AddAsync(entity);
        }

        if (!isManager)
            return Forbid();

        if (entity.TenantId == null || entity.TenantId == Guid.Empty) entity.TenantId = user.TenantId;

        if (await roleRepository.RoleNameExistsAsync(entity.Name, entity.TenantId!.Value))
        {
            return BadRequest(new { message = "Role name is already in use." });
        }

        return await base.AddAsync(entity);
    }

    [UserPermissionAuthorize(UserPermission.ViewRoles | UserPermission.ManageAllRoles)]
    public override async Task<IActionResult> GetAsync()
    {
        var userId = User.GetUserId();

        if (userId == Guid.Empty)
            return Forbid();

        var user = await userRepository.FindByIdAsync(userId);

        if (user != null && user.Role != null && user.Role.Permissions.HasPermission(UserPermission.ViewRoles))
        {
            var result = (await roleRepository.FindAsync()).Where(r => r.TenantId == user.TenantId);
            return Ok(result);
        }

        return await base.GetAsync();
    }

    [UserPermissionAuthorize(UserPermission.EditRole | UserPermission.ManageAllRoles)]
    public override async Task<IActionResult> UpdateAsync([FromBody] Role entity)
    {
        if (entity == null)
            return BadRequest(new { message = "Role data is required." });

        var userId = User.GetUserId();

        if (userId == Guid.Empty)
            return Forbid();

        var existingRole = await roleRepository.FindByIdAsync(entity.Id);

        if (existingRole == null)
            return BadRequest(new { message = "Role not found." });

        var permissions = await userRepository.GetUserPermissionAsync(userId);
        var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllRoles);
        var isManager = permissions.Value.HasPermission(UserPermission.EditRole) && existingRole.TenantId == (await userRepository.FindByIdAsync(userId))?.TenantId;

        if (!isAdmin && !isManager) return Forbid();

        var nameConflict = (await roleRepository.FindAsync()).FirstOrDefault(r => r.Name == entity.Name && r.Id != entity.Id && r.TenantId == entity.TenantId);

        if (nameConflict != null)
        {
            return BadRequest(new { message = "Role name is already in use." });
        }

        return await base.UpdateAsync(entity);
    }

    [UserPermissionAuthorize(UserPermission.ViewRole | UserPermission.ViewRoles | UserPermission.ManageAllRoles)]
    public override async Task<IActionResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { message = "Valid role ID is required." });

        var userId = User.GetUserId();

        if (userId == Guid.Empty)
            return Forbid();

        var existingRole = await roleRepository.FindByIdAsync(id);

        if (existingRole == null)
            return NotFound();

        var permissions = await userRepository.GetUserPermissionAsync(userId);
        var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllRoles);
        var isManager = permissions.Value.HasPermission(UserPermission.ViewRoles) && existingRole.TenantId == (await userRepository.FindByIdAsync(userId))?.TenantId;
        if (!isAdmin && !isManager) return Forbid();

        return await base.GetByIdAsync(id);
    }

    [UserPermissionAuthorize(UserPermission.DeleteRole | UserPermission.ManageAllRoles)]
    public override async Task<IActionResult> DeleteByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { message = "Valid role ID is required." });

        var userId = User.GetUserId();

        if (userId == Guid.Empty)
            return Forbid();

        var permissions = await userRepository.GetUserPermissionAsync(userId);
        var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllRoles);

        var existingRole = await roleRepository.FindByIdAsync(id);

        if (existingRole == null)
            return NotFound();

        var isManager = permissions.Value.HasPermission(UserPermission.DeleteRole) && existingRole.TenantId == (await userRepository.FindByIdAsync(userId))?.TenantId;
        if (!isAdmin && !isManager) return Forbid();
        return await base.DeleteByIdAsync(id);
    }

    [UserPermissionAuthorize(UserPermission.ManageAllRoles)]
    public override async Task<IActionResult> DeleteAllAsync()
    {
        return await base.DeleteAllAsync();
    }
}
