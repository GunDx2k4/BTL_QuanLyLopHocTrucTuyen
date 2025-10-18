using System;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace BTL_QuanLyLopHocTrucTuyen.Authorizations;

public class UserPermissionAuthorizeAttribute(UserPermission requiredPermission) : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public UserPermission RequiredPermission { get; } = requiredPermission;

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
