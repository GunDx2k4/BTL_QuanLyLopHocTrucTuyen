using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.SqlServer;

public class SqlServerMaterialRepository(SqlServerDbContext context) : IMaterialRepository
{
    protected readonly DbSet<Material> _dbSet = context.Materials;
    protected readonly IQueryable<Material> _queryable = context.Materials
        .Include(m => m.Lesson)
        .Include(m => m.Uploader);

    public async Task<Material?> AddAsync(Material entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();

        await context.Entry(entity).Reference(m => m.Lesson).LoadAsync();
        await context.Entry(entity).Reference(m => m.Uploader).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<Material>> FindAsync()
    {
        return await _queryable.ToListAsync();
    }

    public async Task<Material?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<int> UpdateAsync(Material entity)
    {
        _dbSet.Update(entity);
        var updated = await context.SaveChangesAsync();

        await context.Entry(entity).Reference(m => m.Lesson).LoadAsync();
        await context.Entry(entity).Reference(m => m.Uploader).LoadAsync();

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
        var entity = await _queryable.FirstOrDefaultAsync(m => m.Id == id);
        if (entity is null) return 0;
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }
}
