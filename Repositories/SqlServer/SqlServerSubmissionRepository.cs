using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.SqlServer;

public class SqlServerSubmissionRepository(SqlServerDbContext context) : ISubmissionRepository
{
    protected readonly DbSet<Submission> _dbSet = context.Submissions;
    protected readonly IQueryable<Submission> _queryable = context.Submissions
        .Include(s => s.Assignment)
        .Include(s => s.Student);

    public async Task<Submission?> AddAsync(Submission entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();

        await context.Entry(entity).Reference(s => s.Assignment).LoadAsync();
        await context.Entry(entity).Reference(s => s.Student).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<Submission>> FindAsync()
    {
        return await _queryable.ToListAsync();
    }

    public async Task<Submission?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<int> UpdateAsync(Submission entity)
    {
        var existing = await _dbSet.FindAsync(entity.Id);
        if (existing == null) return 0;

        context.Entry(existing).CurrentValues.SetValues(entity);
        _dbSet.Update(existing);
        var updated = await context.SaveChangesAsync();

        await context.Entry(existing).Reference(s => s.Assignment).LoadAsync();
        await context.Entry(existing).Reference(s => s.Student).LoadAsync();

        return updated;
    }
    public async Task<int> UpdateAsync(Submission entity, Submission newEntity)
    {
        return await UpdateAsync(newEntity);
    }

    public async Task<int> DeleteAllAsync()
    {
        var entities = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(Guid id)
    {
        var entity = await _queryable.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null) return 0;
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }
}
