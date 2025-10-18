using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlCourseRepository(MySqlDbContext context) : ICourseRepository
{
    protected readonly DbSet<Course> _dbSet = context.Courses;
    
    public async Task<Course?> AddAsync(Course entity)
    {
        _dbSet.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<Course>> FindAsync()
    {
        var products = await _dbSet.ToListAsync();
        return products;
    }

    public async Task<Course?> FindByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<int> UpdateAsync(Course entity)
    {

        _dbSet.Update(entity);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteAllAsync()
    {
        var entities = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null)
        {
            return 0;
        }
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }

}
