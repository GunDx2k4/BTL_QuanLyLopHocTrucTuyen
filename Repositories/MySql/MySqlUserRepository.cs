using System;
using System.Security.Cryptography;
using System.Text;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlUserRepository(MySqlDbContext context) : IUserRepository
{
    protected readonly DbSet<User> _dbSet = context.Users;
    protected readonly IQueryable<User> _queryable = context.Users
        .Include(u => u.Role)
        .Include(u => u.Tenant);

    public async Task<User?> AddAsync(User entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        
        await context.Entry(entity).Reference(u => u.Role).LoadAsync();
        await context.Entry(entity).Reference(u => u.Tenant).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<User>> FindAsync()
    {
        var users = await _queryable.ToListAsync();
        return users;
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<int> UpdateAsync(User entity)
    {
        _dbSet.Update(entity);
        var updated = await context.SaveChangesAsync();

        await context.Entry(entity).Reference(u => u.Role).LoadAsync();
        await context.Entry(entity).Reference(u => u.Tenant).LoadAsync();
        
        return updated;
    }

    public async Task<int> DeleteAllAsync()
    {
        var users = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(users);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(Guid id)
    {
        var user = await _dbSet.FindAsync(id);
        if (user is null)
        {
            return 0;
        }
        _dbSet.Remove(user);
        return await context.SaveChangesAsync();
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _queryable.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> ValidateUser(string email, string password)
    {
        return await _queryable.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == SecurityHelper.HashPassword(password));
    }

    public async Task<UserPermission?> GetUserPermissionAsync(Guid userId)
    {
        var user = await _queryable.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Role?.Permissions;
    }
    
    public async Task<bool> IsSameTenantAsync(Guid userId, Guid userIdToCheck)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;
        var userToCheck = await _dbSet.FirstOrDefaultAsync(u => u.Id == userIdToCheck);
        if (userToCheck == null) return false;
        if (user.TenantId == null || userToCheck.TenantId == null) return false;
        return user.TenantId == userToCheck.TenantId;
    }

}
