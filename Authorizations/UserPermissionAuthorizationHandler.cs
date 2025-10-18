using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_QuanLyLopHocTrucTuyen.Authorizations;

public class UserPermissionAuthorizationHandler : AuthorizationHandler<UserPermissionAuthorizeAttribute>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserPermissionAuthorizeAttribute requirement)
    {
        var httpContext = (context.Resource as AuthorizationFilterContext)?.HttpContext;

        var userPermissions = context.User.FindFirst(c => c.Type == "Permissions");

        if (userPermissions != null && Enum.TryParse(userPermissions.Value, out Models.Enums.UserPermission permissions))
        {
            if ((permissions & requirement.RequiredPermission) == requirement.RequiredPermission)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
