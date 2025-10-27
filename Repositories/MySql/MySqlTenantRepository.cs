using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlTenantRepository(MySqlDbContext context) : ITenantRepository
{
    protected readonly DbSet<Tenant> _dbSet = context.Tenants;
    public async Task<Tenant?> AddAsync(Tenant entity)
    {
        await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<Tenant?> FindByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<Tenant>> FindAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<int> DeleteAllAsync()
    {
        var tenants = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(tenants);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(Guid id)
    {
        var tenant = await _dbSet.FindAsync(id);
        if (tenant == null) return 0;

        _dbSet.Remove(tenant);
        return await context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(Tenant entity)
    {
        _dbSet.Update(entity);
        return await context.SaveChangesAsync();
    }
}
