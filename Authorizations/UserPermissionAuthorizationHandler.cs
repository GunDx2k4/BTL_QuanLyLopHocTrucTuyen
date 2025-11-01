using System;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_QuanLyLopHocTrucTuyen.Authorizations;

public class UserPermissionAuthorizationHandler(IServiceScopeFactory scopeFactory) : AuthorizationHandler<UserPermissionAuthorizeAttribute>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserPermissionAuthorizeAttribute requirement)
    {
        if (!context.User.Identity!.IsAuthenticated)
        {
            context.Fail(); // → 401 Unauthorized
            return;
        }
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            context.Fail(); // → 403 Forbidden
            return;
        }

        var userRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUserRepository>();
        var userPermissions = await userRepository.GetUserPermissionAsync(userId);

        if (!userPermissions.HasValue)
        {
            context.Fail(); // → 403 Forbidden
            return;
        }

        if (userPermissions.Value.HasPermission(requirement.RequiredPermission))
        {
            context.Succeed(requirement); // → 200 OK
            return;
        }
        context.Fail(); // → 403 Forbidden
    }
}
