using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlAssignmentRepository(MySqlDbContext context) : IAssignmentRepository
{
    protected readonly DbSet<Assignment> _dbSet = context.Assignments;
    protected readonly IQueryable<Assignment> _queryable = context.Assignments
        .Include(a => a.Lesson)
        .Include(a => a.Submissions);

    public async Task<Assignment?> AddAsync(Assignment entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();

        await context.Entry(entity).Reference(a => a.Lesson).LoadAsync();
        await context.Entry(entity).Collection(a => a.Submissions).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<Assignment>> FindAsync()
    {
        return await _queryable.ToListAsync();
    }

    public async Task<Assignment?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<int> UpdateAsync(Assignment entity)
    {
        var existing = await _dbSet.FindAsync(entity.Id);
        if (existing == null) return 0;

        context.Entry(existing).CurrentValues.SetValues(entity);
        _dbSet.Update(existing);
        var updated = await context.SaveChangesAsync();

        await context.Entry(existing).Reference(a => a.Lesson).LoadAsync();
        await context.Entry(existing).Collection(a => a.Submissions).LoadAsync();

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
        var entity = await _queryable.FirstOrDefaultAsync(a => a.Id == id);
        if (entity is null) return 0;
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }
}
