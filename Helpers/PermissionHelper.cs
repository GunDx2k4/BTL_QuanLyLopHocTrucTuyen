using System;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Helpers;

public static class PermissionHelper
{
    public static bool HasPermission(this User user, UserPermission requiredPermission)
    {
        if (user.Role == null)
        {
            return false;
        }

        var userPermissions = user.Role.Permissions;

        return (userPermissions & requiredPermission) == requiredPermission;
    }
}
