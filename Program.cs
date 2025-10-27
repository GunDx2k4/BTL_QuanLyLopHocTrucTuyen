using System.Net;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Middlewares;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;
using BTL_QuanLyLopHocTrucTuyen.Repositories.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = async ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                ctx.HttpContext.Response.Redirect("/Home/Login");
            },
            OnRedirectToAccessDenied = async ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                
                await ctx.HttpContext.RedirectToHomePage();
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});


builder.Services.AddSingleton<IAuthorizationHandler, UserPermissionAuthorizationHandler>();

builder.Services.AddDbContext<ApplicationDbContext, MySqlDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// builder.Services.AddDbContext<ApplicationDbContext, SqlServerDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

builder.Services.AddScoped<ICourseRepository, MySqlCourseRepository>();
builder.Services.AddScoped<IUserRepository, MySqlUserRepository>();
builder.Services.AddScoped<ITenantRepository, MySqlTenantRepository>();

// builder.Services.AddScoped<ICourseRepository, SqlServerCourseRepository>();
// builder.Services.AddScoped<IUserRepository, SqlServerUserRepository>();
// builder.Services.AddScoped<ITenantRepository, SqlServerTenantRepository>();

builder.Services.AddMemoryCache();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SingleSessionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();