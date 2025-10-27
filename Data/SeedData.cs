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
                Role = adminRole
            };

            var userManager = new User
            {
                FullName = "Nguyen Van A",
                Email = "nguyenvana@gmail.com",
                PasswordHash = SecurityHelper.HashPassword("123456"),
            };
            
            var userInstructor = new User
            {
                FullName = "Nguyen Van C",
                Email = "nguyenvanc@gmail.com",
                PasswordHash = SecurityHelper.HashPassword("123456"),
            };
            
            var userManager2 = new User
            {
                FullName = "Nguyen Van B",
                Email = "nguyenvanb@gmail.com",
                PasswordHash = SecurityHelper.HashPassword("123456"),
            };

            var tenantDemo = new Tenant
            {
                OwnerId = userManager.Id,
                Name = "Demo",
            };
            var tenantDemo2 = new Tenant
            {
                OwnerId = userManager2.Id,
                Name = "Demo2",
            };

            var manager = new Role
            {
                Name = "Manager",
                Description = "Manager tenant Demo",
                Permissions = UserPermission.Manager,
            };
            var manager2 = new Role
            {
                Name = "Manager2",
                Description = "Manager tenant Demo2",
                Permissions = UserPermission.Manager,
            };
            var instructor = new Role
            {
                Name = "Instructor",
                Description = "Instructor role",
                Permissions = UserPermission.Instructor,
            };

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(adminRole, manager, manager2, instructor);
                context.SaveChanges();
            }
            
            if (!context.Users.Any())
            {
                context.Users.AddRange(userAdmin, userManager, userManager2, userInstructor);
                context.SaveChanges();
            }

            if (!context.Tenants.Any())
            {
                context.Tenants.AddRange(tenantDemo, tenantDemo2);
                manager.TenantId = tenantDemo.Id;
                manager2.TenantId = tenantDemo2.Id;
                instructor.TenantId = tenantDemo.Id;
                context.Roles.UpdateRange(manager, manager2, instructor);
                userManager.RoleId = manager.Id;
                userManager2.RoleId = manager2.Id;
                userInstructor.RoleId = instructor.Id;
                userManager.TenantId = tenantDemo.Id;
                userManager2.TenantId = tenantDemo2.Id;
                userInstructor.TenantId = tenantDemo.Id;
                context.Users.UpdateRange(userManager, userManager2, userInstructor);
                context.SaveChanges();

            }

        }

    }
}
