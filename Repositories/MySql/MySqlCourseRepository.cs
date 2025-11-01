using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlCourseRepository(MySqlDbContext context) : ICourseRepository
{
    protected readonly DbSet<Course> _dbSet = context.Courses;
    protected readonly IQueryable<Course> _queryable = context.Courses
        .Include(c => c.Instructor)
        .Include(c => c.Tenant)
        .Include(c => c.Lessons)
        .Include(c => c.Enrollments);

    public async Task<Course?> AddAsync(Course entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();

        // Load navigation properties
        await context.Entry(entity).Reference(c => c.Instructor).LoadAsync();
        await context.Entry(entity).Reference(c => c.Tenant).LoadAsync();
        await context.Entry(entity).Collection(c => c.Lessons).LoadAsync();
        await context.Entry(entity).Collection(c => c.Enrollments).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<Course>> FindAsync()
    {
        var courses = await _queryable.ToListAsync();
        return courses;
    }

    public async Task<Course?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> UpdateAsync(Course entity)
    {
        _dbSet.Update(entity);
        var updated = await context.SaveChangesAsync();

        await context.Entry(entity).Reference(c => c.Instructor).LoadAsync();
        await context.Entry(entity).Reference(c => c.Tenant).LoadAsync();
        await context.Entry(entity).Collection(c => c.Lessons).LoadAsync();
        await context.Entry(entity).Collection(c => c.Enrollments).LoadAsync();
        
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
        var entity = await _dbSet.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
        {
            return 0;
        }
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }

    public async Task<bool> IsSameTenantAsync(Guid userId, Guid courseId)
    {
        var course = await _queryable.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null)
            return false;

        var instructor = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (instructor == null)
            return false;

        return course.TenantId == instructor.TenantId;
    }

}
