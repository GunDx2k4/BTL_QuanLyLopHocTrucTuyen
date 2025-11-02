using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql;

public class MySqlEnrollmentRepository(MySqlDbContext context) : IEnrollmentRepository
{
    protected readonly DbSet<Enrollment> _dbSet = context.Enrollments;
    protected readonly IQueryable<Enrollment> _queryable = context.Enrollments
        .Include(e => e.User)
        .Include(e => e.Course).ThenInclude(c => c.Instructor);

    public async Task<Enrollment?> AddAsync(Enrollment entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();

        await context.Entry(entity).Reference(e => e.User).LoadAsync();
        await context.Entry(entity).Reference(e => e.Course).LoadAsync();

        return result.Entity;
    }

    public async Task<IEnumerable<Enrollment>> FindAsync()
    {
        return await _queryable.ToListAsync();
    }

    public async Task<Enrollment?> FindByIdAsync(Guid id)
    {
        return await _queryable.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<int> UpdateAsync(Enrollment entity)
    {
        var existing = await _dbSet.FindAsync(entity.Id);
        if (existing == null) return 0;

        context.Entry(existing).CurrentValues.SetValues(entity);
        _dbSet.Update(existing);
        var updated = await context.SaveChangesAsync();

        await context.Entry(existing).Reference(e => e.User).LoadAsync();
        await context.Entry(existing).Reference(e => e.Course).LoadAsync();

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
        var entity = await _queryable.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return 0;
        _dbSet.Remove(entity);
        return await context.SaveChangesAsync();
    }


    public async Task<List<Enrollment>> GetEnrollmentsByUserIdAsync(Guid userId)
    {
        return await context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                    .ThenInclude(l => l.Assignments)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(Guid enrollmentId)
    {
        return await context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                    .ThenInclude(l => l.Assignments)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                    .ThenInclude(l => l.Materials)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);
    }

    public async Task<bool> EnrollCourseAsync(Guid userId, Guid courseId)
    {
        try
        {
            // Kiểm tra đã đăng ký chưa
            var existingEnrollment = await context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existingEnrollment != null)
                return false; // Đã đăng ký rồi

            // Kiểm tra khóa học có tồn tại không
            var course = await context.Courses.FindAsync(courseId);
            if (course == null)
                return false;

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Enrolled
            };

            context.Enrollments.Add(enrollment);
            return await context.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DropCourseAsync(Guid enrollmentId)
    {
        try
        {
            var enrollment = await context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
                return false;

            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.DroppedAt = DateTime.UtcNow;

            return await context.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Course>> GetAvailableCoursesAsync(Guid userId, Guid tenantId)
    {
        // Lấy danh sách courseId đã đăng ký (kể cả dropped)
        var enrolledCourseIds = await context.Enrollments
            .Where(e => e.UserId == userId)
            .Select(e => e.CourseId)
            .ToListAsync();

        // Lấy các khóa học chưa đăng ký trong cùng tenant
        return await context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Where(c => c.TenantId == tenantId && !enrolledCourseIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentByUserAndCourseAsync(Guid userId, Guid courseId)
    {
        return await context.Enrollments
            .Include(e => e.Course).ThenInclude(c => c.Instructor)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
    }

}
