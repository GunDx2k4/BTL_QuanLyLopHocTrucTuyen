using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories;

public interface IRoleRepository : IEntityRepository<Role>
{
    Role DefaultRole { get; }
    
    Task<bool> RoleNameExistsAsync(string name, Guid tenantId);

}
