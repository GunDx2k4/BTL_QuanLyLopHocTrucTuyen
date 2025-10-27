using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories;

public interface IUserRepository : IEntityRepository<User>
{
    public Task<User?> FindByEmailAsync(string email);

    public Task<User?> ValidateUser(string email, string password);

    public Task<UserPermission?> GetUserPermissionAsync(Guid userId);

    public Task<bool> IsSameTenantAsync(Guid userId, Guid userIdToCheck);
}
