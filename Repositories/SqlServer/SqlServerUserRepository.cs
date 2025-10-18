using System;
using System.Security.Cryptography;
using System.Text;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.SqlServer;

public class SqlServerUserRepository(SqlServerDbContext context) : IUserRepository
{
    protected readonly DbSet<User> _dbSet = context.Users;

    public async Task<User?> AddAsync(User entity)
    {
        _dbSet.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<User>> FindAsync()
    {
        var users = await _dbSet.ToListAsync();
        return users;
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<int> UpdateAsync(User entity)
    {

        _dbSet.Update(entity);
        return await context.SaveChangesAsync();
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

    public async Task<User?> ValidateUser(string email, string password)
    {
        var passwordHash = Convert.ToBase64String(SHA512.HashData(Encoding.UTF8.GetBytes(password)));

        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);
    }
}
