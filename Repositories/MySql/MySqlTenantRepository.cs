using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlTenantRepository(MySqlDbContext context) : ITenantRepository
{
    protected readonly DbSet<Tenant> _dbSet = context.Tenants;
    protected readonly IQueryable<Tenant> _queryable = context.Tenants
        .Include(t => t.Owner)
        .Include(t => t.Roles)
        .Include(t => t.Users)
        .Include(t => t.Courses);

    public async Task<Tenant?> AddAsync(Tenant entity)
    {
        await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        await context.Entry(entity).Reference(t => t.Owner).LoadAsync();
        await context.Entry(entity).Collection(t => t.Roles).LoadAsync();
        await context.Entry(entity).Collection(t => t.Users).LoadAsync();
        return entity;
    }

    public async Task<Tenant?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tenant>> FindAsync()
    {
        return await _queryable.ToListAsync();
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

    public async Task<Tenant?> FindTenantByOwnerIdAsync(Guid ownerId)
    {
        return await _queryable.FirstOrDefaultAsync(t => t.OwnerId == ownerId);
    }

    public async Task<bool> IsOwnerTenantAsync(Guid userId, Guid tenantId)
    {
        var tenant = await _dbSet.FirstOrDefaultAsync(t => t.Id == tenantId && t.OwnerId == userId);
        return tenant != null;
    }

    public async Task<Role?> GetRoleInstructorDefaultAsync(Guid tenantId)
    {
        var tenant = await _dbSet.Include(t => t.Roles)
                                 .FirstOrDefaultAsync(t => t.Id == tenantId);
        return tenant?.Roles.FirstOrDefault(r => r.Name == "Instructor");
    }

    public async Task<Role?> GetRoleStudentDefaultAsync(Guid tenantId)
    {
        var tenant = await _dbSet.Include(t => t.Roles)
                                 .FirstOrDefaultAsync(t => t.Id == tenantId);
        return tenant?.Roles.FirstOrDefault(r => r.Name == "Student");
    }
}
