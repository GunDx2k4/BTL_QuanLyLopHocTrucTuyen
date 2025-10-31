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

    public static async Task RedirectToHomePage(this HttpContext context)
    {
        var userId = context.User.GetUserId();
        if (userId == Guid.Empty)
        {
            context.Response.Redirect("/");
            return;
        }

        var userRepository = context.RequestServices.GetRequiredService<IUserRepository>();
        var permissions = await userRepository.GetUserPermissionAsync(userId);

        if (permissions == null)
        {
            context.Response.Redirect("/");
            return;
        }
        if (permissions == UserPermission.None)
        {
            context.Response.Redirect("/");
            return;
        }
        if (permissions.Value.HasFlag(UserPermission.AdminUser))
        {
            context.Response.Redirect("/Admin");
            return;
        }
        if (permissions.Value.HasFlag(UserPermission.ManagerUser))
        {
            context.Response.Redirect("/Manager");
            return;
        }
        if (permissions.Value.HasFlag(UserPermission.InstructorUser))
        {
            context.Response.Redirect("/Instructor");
            return;
        }
        if (permissions.Value.HasFlag(UserPermission.StudentUser))
        {
            context.Response.Redirect("/Student");
            return;
        }
        context.Response.Redirect("/");
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

        if (permissions == UserPermission.None) return controller.View("RegisterTenant");

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
