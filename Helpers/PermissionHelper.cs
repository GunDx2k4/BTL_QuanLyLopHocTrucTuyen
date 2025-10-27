using System;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Helpers;

public static class PermissionHelper
{
    public static bool HasPermission(this UserPermission source, UserPermission toCheck)
    {
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
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            context.Response.Redirect("/");
            return;
        }

        var userRepository = context.RequestServices.GetRequiredService<IUserRepository>();
        var permissions = await userRepository.GetUserPermissionAsync(Guid.Parse(userId));

        switch (permissions)
        {
            case null:
                context.Response.Redirect("/");
                break;
            case UserPermission.Administrator:
                context.Response.Redirect("/Admin");
                break;
            case UserPermission.Manager:
                context.Response.Redirect("/Manager");
                break;
            case UserPermission.Instructor:
                context.Response.Redirect("/Instructor");
                break;
            case UserPermission.Student:
                context.Response.Redirect("/Student");
                break;
            default:
                context.Response.Redirect("/");
                break;
        }
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

        switch (permissions)
        {
            case UserPermission.Administrator:
                return controller.RedirectToAction("Index", "Admin");
            case UserPermission.Manager:
                return controller.RedirectToAction("Index", "Manager");
            case UserPermission.Instructor:
                return controller.RedirectToAction("Index", "Instructor");
            case UserPermission.Student:
                return controller.RedirectToAction("Index", "Student");
            default:
                return controller.View();
        }
    }
}
