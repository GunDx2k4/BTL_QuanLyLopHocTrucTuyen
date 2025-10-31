using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories;

public interface ICourseRepository : IEntityRepository<Course>
{
    public Task<bool> IsSameTenantAsync(Guid userId, Guid courseId);

}
