using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlLessonRepository(MySqlDbContext context) : ILessonRepository
{
    protected readonly DbSet<Lesson> _dbSet = context.Lessons;
    protected readonly IQueryable<Lesson> _queryable = context.Lessons
        .Include(l => l.Course)
        .Include(l => l.Assignments)
        .Include(l => l.Materials);

    public async Task<Lesson?> AddAsync(Lesson entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();

        await context.Entry(entity).Reference(l => l.Course).LoadAsync();
        await context.Entry(entity).Collection(l => l.Assignments).LoadAsync();
        await context.Entry(entity).Collection(l => l.Materials).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<Lesson>> FindAsync()
    {
        return await _queryable.ToListAsync();
    }

    public async Task<Lesson?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<int> UpdateAsync(Lesson entity)
    {
        var existing = await _dbSet.FindAsync(entity.Id);
        if (existing == null) return 0;

        context.Entry(existing).CurrentValues.SetValues(entity);
        _dbSet.Update(existing);
        var updated = await context.SaveChangesAsync();

        await context.Entry(existing).Reference(l => l.Course).LoadAsync();
        await context.Entry(existing).Collection(l => l.Assignments).LoadAsync();
        await context.Entry(existing).Collection(l => l.Materials).LoadAsync();

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
        var entity = await _queryable.FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null) return 0;
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }
}
