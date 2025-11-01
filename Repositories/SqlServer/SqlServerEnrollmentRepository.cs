using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.SqlServer;

public class SqlServerEnrollmentRepository(SqlServerDbContext context) : IEnrollmentRepository
{
    protected readonly DbSet<Enrollment> _dbSet = context.Enrollments;
    protected readonly IQueryable<Enrollment> _queryable = context.Enrollments
        .Include(e => e.User)
        .Include(e => e.Course);

    public async Task<Enrollment?> AddAsync(Enrollment entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();

        await context.Entry(entity).Reference(e => e.User).LoadAsync();
        await context.Entry(entity).Reference(e => e.Course).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<Enrollment>> FindAsync()
    {
        return await _queryable.ToListAsync();
    }

    public async Task<Enrollment?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<int> UpdateAsync(Enrollment entity)
    {
        _dbSet.Update(entity);
        var updated = await context.SaveChangesAsync();

        await context.Entry(entity).Reference(e => e.User).LoadAsync();
        await context.Entry(entity).Reference(e => e.Course).LoadAsync();

        return updated;
    }

    public async Task<int> DeleteAllAsync()
    {
        var entities = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(Guid id)
    {
        var entity = await _queryable.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return 0;
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }
}
