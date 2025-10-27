using System;
using System.Security.Claims;

namespace BTL_QuanLyLopHocTrucTuyen.Helpers;

public static class ConverterHelper
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    public static Guid GetSessionId(this ClaimsPrincipal user)
    {
        var sessionIdClaim = user.FindFirst("SessionId")?.Value;
        return sessionIdClaim != null ? Guid.Parse(sessionIdClaim) : Guid.Empty;
    }

}
