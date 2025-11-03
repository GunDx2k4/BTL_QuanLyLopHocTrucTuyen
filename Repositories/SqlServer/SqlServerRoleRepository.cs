using System;
using System.Linq;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.SqlServer;

public class SqlServerRoleRepository(SqlServerDbContext context) : IRoleRepository
{
    protected readonly DbSet<Role> _dbSet = context.Roles;
    protected readonly IQueryable<Role> _queryable = context.Roles.Include(r => r.Tenant);

    private Role? _defaultRole;
    public Role DefaultRole => _defaultRole ??= _queryable.First(r => r.Permissions == UserPermission.None);

    public async Task<Role?> AddAsync(Role entity)
    {
        _dbSet.Add(entity);
        await context.SaveChangesAsync();

        await context.Entry(entity).Reference(u => u.Tenant).LoadAsync();

        return entity;

    }

    public async Task<Role?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Role>> FindAsync()
    {
        return await _queryable.ToListAsync();
    }

    public async Task<int> DeleteAllAsync()
    {
        var roles = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(roles);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(Guid id)
    {
        var role = await _dbSet.FindAsync(id);
        if (role == null) return 0;

        _dbSet.Remove(role);

        return await context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(Role entity)
    {
        var existing = await _dbSet.FindAsync(entity.Id);
        if (existing == null) return 0;

        context.Entry(existing).CurrentValues.SetValues(entity);
        _dbSet.Update(existing);
        var updated = await context.SaveChangesAsync();

        await context.Entry(existing).Reference(u => u.Tenant).LoadAsync();

        return updated;
    }

    public async Task<int> UpdateAsync(Role entity, Role newEntity)
    {
        return await UpdateAsync(newEntity);
    }

    public async Task<bool> RoleNameExistsAsync(string name, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var normalized = name.Trim().ToLower();
        var q = _queryable.Where(r => r.TenantId == tenantId);
        return await q.AnyAsync(r => r.Name.ToLower() == normalized);
    }
}
