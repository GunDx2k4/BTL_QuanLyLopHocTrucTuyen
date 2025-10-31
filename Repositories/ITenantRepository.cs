using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories;

public interface ITenantRepository : IEntityRepository<Tenant>
{
    Task<Tenant?> FindTenantByOwnerIdAsync(Guid ownerId);

    Task<bool> IsOwnerTenantAsync(Guid userId, Guid tenantId);

    Task<Role?> GetRoleInstructorDefaultAsync(Guid tenantId);
    Task<Role?> GetRoleStudentDefaultAsync(Guid tenantId);


}
