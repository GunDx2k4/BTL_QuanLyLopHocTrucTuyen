using System;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Helpers;

public static class PermissionHelper
{
    public static bool HasPermission(this UserPermission source, UserPermission toCheck)
    {
        if (source == toCheck)
            return true;
        return (source & toCheck) != 0;
    }

    public static UserPermission AddPermission(this UserPermission source, UserPermission toAdd)
    {
        return source | toAdd;
    }

    public static UserPermission RemovePermission(this UserPermission source, UserPermission toRemove)
    {
        return source & ~toRemove;
    }

    public static async Task<IActionResult> RedirectToHomePage(this Controller controller)
    {
        if (!controller.HttpContext.User.Identity!.IsAuthenticated) return controller.View();

        var userId = controller.HttpContext.User.GetUserId();

        if (userId == Guid.Empty) return controller.View();

        var userRepository = controller.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await userRepository.FindByIdAsync(userId);
        if (user == null) return controller.View();
        
        if (user.Role == null) return controller.View();

        var permissions = user.Role.Permissions;

        if (user.Tenant == null && permissions != UserPermission.Administrator) return controller.View("RegisterTenant");

        if (permissions == UserPermission.None) return controller.View();

        if (permissions.HasFlag(UserPermission.AdminUser))
        {
            return controller.RedirectToAction("Index", "Admin");
        }
        if (permissions.HasFlag(UserPermission.ManagerUser))
        {
            return controller.RedirectToAction("Index", "Manager");
        }
        if (permissions.HasFlag(UserPermission.InstructorUser))
        {
            return controller.RedirectToAction("Index", "Instructor");
        }
        if (permissions.HasFlag(UserPermission.StudentUser))
        {
            return controller.RedirectToAction("Index", "Student");
        }
        
        return controller.View();
    }
}
