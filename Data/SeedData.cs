using System;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Data;

public class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
        {
            var defaultRole = new Role
            {
                Name = "Default",
                Description = "Default role with no permissions",
                Permissions = UserPermission.None,
            };

            var adminRole = new Role
            {
                Name = "Admin",
                Description = "Administrator role with full permissions system",
                Permissions = UserPermission.Administrator,
            };
            var userAdmin = new User
            {
                FullName = "Nguyen Xuan Dung",
                Email = "l7ungdzk4@gmail.com",
                PasswordHash = SecurityHelper.HashPassword("17102004"),
                RoleId = adminRole.Id
            };

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(defaultRole, adminRole);
                context.SaveChanges();
            }
            
            if (!context.Users.Any())
            {
                context.Users.AddRange(userAdmin);
                context.SaveChanges();
            }


        }

    }
}
