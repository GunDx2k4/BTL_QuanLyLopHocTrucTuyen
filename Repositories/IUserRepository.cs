using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories;

public interface IUserRepository : IEntityRepository<User>
{
    public Task<User?> ValidateUser(string email, string password);
}
