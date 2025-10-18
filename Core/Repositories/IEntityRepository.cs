using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Core.Repositories;

public interface IEntityRepository<T> where T : IEntity
{
    Task<T?> AddAsync(T entity);
    Task<T?> FindByIdAsync(Guid id);
    Task<IEnumerable<T>> FindAsync();
    Task<int> DeleteAllAsync();
    Task<int> DeleteByIdAsync(Guid id);
    Task<int> UpdateAsync(T entity);
}
