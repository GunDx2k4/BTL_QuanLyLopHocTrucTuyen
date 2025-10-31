using System;
using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API
{
    [Route("api/courses")]
    public class CourseApiController(ICourseRepository courseRepository, IUserRepository userRepository) : CrudApiController<Course>(courseRepository)
    {
        [UserPermissionAuthorize(UserPermission.CreateCourse | UserPermission.ManageAllTenants | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> AddAsync([FromBody] Course entity)
        {
            if (entity == null)
                return BadRequest(new { message = "Course data is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var user = await userRepository.FindByIdAsync(userId);

            if (user == null || user.Role == null)
                return Forbid();

            var permissions = user.Role.Permissions;
            var isAdmin = permissions.HasPermission(UserPermission.ManageAllTenants) || permissions.HasPermission(UserPermission.ManageAllUsers);
            var isManager = permissions.HasPermission(UserPermission.CreateCourse) && (user.TenantId != null && user.TenantId != Guid.Empty);

            // Validate instructor exists
            var instructor = await userRepository.FindByIdAsync(entity.InstructorId);
            if (instructor == null)
                return BadRequest(new { message = "Instructor not found." });

            if (isAdmin)
            {
                if (entity.TenantId == Guid.Empty)
                {
                    return BadRequest(new { message = "TenantId is required." });
                }

                // ensure instructor belongs to tenant
                if (instructor.TenantId != entity.TenantId)
                    return BadRequest(new { message = "Instructor does not belong to the specified tenant." });

                return await base.AddAsync(entity);
            }

            if (!isManager)
                return Forbid();

            if (entity.TenantId == Guid.Empty) entity.TenantId = user.TenantId!.Value;

            if (instructor.TenantId != entity.TenantId)
                return BadRequest(new { message = "Instructor does not belong to the course tenant." });

            return await base.AddAsync(entity);
        }

        [UserPermissionAuthorize(UserPermission.ViewCourses | UserPermission.ManageAllTenants | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> GetAsync()
        {
            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var user = await userRepository.FindByIdAsync(userId);
            if (user != null && user.Role != null && user.Role.Permissions.HasPermission(UserPermission.ViewCourses))
            {
                var result = (await courseRepository.FindAsync()).Where(c => c.TenantId == user.TenantId);
                return Ok(result);
            }

            return await base.GetAsync();
        }

        [UserPermissionAuthorize(UserPermission.EditCourse | UserPermission.ManageAllTenants | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> UpdateAsync([FromBody] Course entity)
        {
            if (entity == null)
                return BadRequest(new { message = "Course data is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var existingCourse = await courseRepository.FindByIdAsync(entity.Id);

            if (existingCourse == null)
                return BadRequest(new { message = "Course not found." });

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllTenants) || permissions.Value.HasPermission(UserPermission.ManageAllUsers);
            var isManager = permissions.Value.HasPermission(UserPermission.EditCourse) && await courseRepository.IsSameTenantAsync(userId, entity.Id);

            if (!isAdmin && !isManager) return Forbid();

            // Validate instructor belongs to tenant
            var instructor = await userRepository.FindByIdAsync(entity.InstructorId);
            if (instructor == null || instructor.TenantId != entity.TenantId)
            {
                return BadRequest(new { message = "Instructor is invalid or does not belong to the course tenant." });
            }

            return await base.UpdateAsync(entity);
        }

        [UserPermissionAuthorize(UserPermission.ViewCourse | UserPermission.ViewCourses | UserPermission.ManageAllTenants | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Valid course ID is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var course = await courseRepository.FindByIdAsync(id);

            if (course == null)
                return NotFound();

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllTenants) || permissions.Value.HasPermission(UserPermission.ManageAllUsers);
            var isManager = permissions.Value.HasPermission(UserPermission.ViewCourses) &&  await courseRepository.IsSameTenantAsync(userId, course.Id);
            if (!isAdmin && !isManager) return Forbid();

            return await base.GetByIdAsync(id);
        }

        [UserPermissionAuthorize(UserPermission.DeleteCourse | UserPermission.ManageAllTenants | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> DeleteByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Valid course ID is required." });

            var userId = User.GetUserId();

            if (userId == Guid.Empty)
                return Forbid();

            var permissions = await userRepository.GetUserPermissionAsync(userId);
            var isAdmin = permissions.Value.HasPermission(UserPermission.ManageAllTenants) || permissions.Value.HasPermission(UserPermission.ManageAllUsers);

            var existingCourse = await courseRepository.FindByIdAsync(id);

            if (existingCourse == null)
                return NotFound();

            var isManager = permissions.Value.HasPermission(UserPermission.DeleteCourse) && await courseRepository.IsSameTenantAsync(userId, existingCourse.Id);
            if (!isAdmin && !isManager) return Forbid();
            return await base.DeleteByIdAsync(id);
        }

        [UserPermissionAuthorize(UserPermission.ManageAllTenants | UserPermission.ManageAllUsers)]
        public override async Task<IActionResult> DeleteAllAsync()
        {
            return await base.DeleteAllAsync();
        }
    }
}
