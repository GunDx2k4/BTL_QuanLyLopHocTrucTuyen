using System;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace BTL_QuanLyLopHocTrucTuyen.Middlewares;

public class SingleSessionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IMemoryCache cache)
    {
        if (!context.User.Identity!.IsAuthenticated)
        {
            await next(context);
            return;
        }

        var userId = context.User.GetUserId();
        if (userId == Guid.Empty)
        {
            await next(context);
            return;
        }

        if (cache.TryGetValue(userId, out Guid sessionId))
        {
            var sessionClaim = context.User.GetSessionId();
            if (sessionClaim == Guid.Empty || sessionClaim != sessionId)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                context.Response.Redirect("/Home/Login");
                return;
            }
        }
        else
        {
            var sessionClaim = context.User.GetSessionId();
            if (sessionClaim == Guid.Empty)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (context.Request.Path.StartsWithSegments("/api"))
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                else
                    context.Response.Redirect("/Home/Login");
                cache.Remove(userId);
                return;
            }
            cache.Set(userId, sessionClaim);
            await next(context);
            return;
        }

        await next(context);
    }

}
