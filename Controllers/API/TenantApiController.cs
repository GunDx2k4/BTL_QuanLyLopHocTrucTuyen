using System.Globalization;
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
    [Route("api/tenants")]
    public class TenantApiController(ITenantRepository tenantRepository, IRoleRepository roleRepository, IUserRepository userRepository) : CrudApiController<Tenant>(tenantRepository)
    {
        [UserPermissionAuthorize(UserPermission.ManageAllTenants)]
        public override async Task<IActionResult> AddAsync([FromBody] Tenant entity)
        {
            return await base.AddAsync(entity);
        }

        [UserPermissionAuthorize(UserPermission.ViewTenant | UserPermission.ManageAllTenants)]
        public override async Task<IActionResult> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Valid tenant ID is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllTenants);
            var isManager = permissions.Value.HasPermission(UserPermission.ViewTenant) && await tenantRepository.IsOwnerTenantAsync(userId, id);
            if (!isAdmin && !isManager) return Forbid();
            return await base.GetByIdAsync(id);
        }

        [UserPermissionAuthorize(UserPermission.ManageAllTenants)]
        public override async Task<IActionResult> GetAsync()
        {
            return await base.GetAsync();
        }

        [UserPermissionAuthorize(UserPermission.ManageAllTenants)]
        public override async Task<IActionResult> UpdateAsync([FromBody] Tenant entity)
        {
            return await base.UpdateAsync(entity);
        }

        [UserPermissionAuthorize(UserPermission.ManageAllTenants)]
        public override async Task<IActionResult> DeleteByIdAsync(Guid id)
        {
            return await base.DeleteByIdAsync(id);
        }

        [UserPermissionAuthorize(UserPermission.ManageAllTenants)]
        public override async Task<IActionResult> DeleteAllAsync()
        {
            return await base.DeleteAllAsync();
        }

        [HttpPost("register")]
        [UserPermissionAuthorize(UserPermission.None)]
        public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();
            var user = await userRepository.FindByIdAsync(userId);

            if (user == null)
                return Forbid();

            if (user.TenantId != null)
            {
                return BadRequest(new { message = "User already belongs to a tenant." });
            }

            var (tenant, roleManager, roleInstructorDefault, roleStudentDefault) = await user.SetupNewTenant(request.Name, request.Plan);
            
            await tenantRepository.AddAsync(tenant);
            await roleRepository.AddAsync(roleManager);
            await roleRepository.AddAsync(roleInstructorDefault);
            await roleRepository.AddAsync(roleStudentDefault);

            user.Tenant = tenant;
            user.Role = roleManager;

            await userRepository.UpdateAsync(user);

            return Ok(new { message = "Tenant registered successfully" });
        }

        [HttpGet("{id:guid}/lessons")]
        [UserPermissionAuthorize(UserPermission.ViewTenant | UserPermission.ManageAllTenants)]
        public async Task<IActionResult> GetLessonsForWeek(Guid id, [FromQuery] string week)
        {
            if (string.IsNullOrEmpty(week))
                return BadRequest("Week parameter is required");

            if (!ConverterHelper.TryParseIsoWeek(week, out var startDate))
            {
                return BadRequest("Invalid week format. Use YYYY-Www format");
            }

            if (id == Guid.Empty)
                return BadRequest(new { message = "Valid tenant ID is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllTenants);
            var isManager = permissions.Value.HasPermission(UserPermission.ViewTenant) && await tenantRepository.IsOwnerTenantAsync(userId, id);
            if (!isAdmin && !isManager) return Forbid();

            var endDate = startDate.AddDays(7);
            var tenant = await tenantRepository.FindByIdAsync(id);
            if (tenant == null)
                return NotFound(new { message = "Tenant not found." });
            var lessons = tenant.Courses
                .SelectMany(c => c.Lessons)
                .Where(l => l.BeginTime >= startDate && l.BeginTime < endDate);
            return Ok(lessons);
        }

        [HttpPut("{id:guid}/plan")]
        [UserPermissionAuthorize(UserPermission.ViewTenant | UserPermission.ManageAllTenants)]
        public async Task<IActionResult> UpdateTenantPlanAsync(Guid id, [FromBody] UpdateTenantPlanViewModel request)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Valid tenant ID is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllTenants);
            var isManager = permissions.Value.HasPermission(UserPermission.ViewTenant) && await tenantRepository.IsOwnerTenantAsync(userId, id);
            if (!isAdmin && !isManager) return Forbid();

            var tenant = await tenantRepository.FindByIdAsync(id);
            if (tenant == null)
                return NotFound(new { message = "Tenant not found." });

            tenant.Plan = request.NewPlan;
            if (request.NewPlan == PlanType.Free)
            {
                tenant.EndTime = null;
            }
            else
            {
                var duration = request.DurationInMonths > 0 ? request.DurationInMonths : 1;
                tenant.EndTime = tenant.EndTime == null || tenant.EndTime < DateTime.UtcNow
                    ? DateTime.UtcNow.AddMonths(duration)
                    : tenant.EndTime.Value.AddMonths(duration);
            }
            await tenantRepository.UpdateAsync(tenant);

            return Ok(new { message = "Tenant plan updated successfully." });
        }
    }
    
}
