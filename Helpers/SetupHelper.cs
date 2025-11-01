using System;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;

namespace BTL_QuanLyLopHocTrucTuyen.Helpers;

public static class SetupHelper
{
    public static async Task<(Tenant, Role, Role, Role)> SetupNewTenant(this User user, string tenantName, PlanType planType, DateTime? endTime = null)
    {
        var tenant = new Tenant
        {
            Name = tenantName,
            Plan = planType,
            EndTime = endTime,
            OwnerId = user.Id
        };

        var roleManager = new Role
        {
            Name = "Manager",
            Description = "Role for tenant manager",
            Permissions = UserPermission.Manager,
            Tenant = tenant
        };

        var roleInstructorDefault = new Role
        {
            Name = "Instructor",
            Description = "Default role for instructors",
            Permissions = UserPermission.Instructor,
            Tenant = tenant,
        };

        var roleStudentDefault = new Role
        {
            Name = "Student",
            Description = "Default role for students",
            Permissions = UserPermission.Student,
            Tenant = tenant,
        };

        return (tenant, roleManager, roleInstructorDefault, roleStudentDefault);
    }
}
