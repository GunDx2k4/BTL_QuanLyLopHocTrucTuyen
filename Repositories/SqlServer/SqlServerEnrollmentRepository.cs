using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.SqlServer;

public class SqlServerEnrollmentRepository : IEnrollmentRepository
{
    private readonly SqlServerDbContext _context;

    public SqlServerEnrollmentRepository(SqlServerDbContext context)
    {
        _context = context;
    }

    public async Task<List<Enrollment>> GetEnrollmentsByUserIdAsync(Guid userId)
    {
        return await _context.Enrollments
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
        return await _context.Enrollments
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
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existingEnrollment != null)
                return false;

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return false;

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Enrolled
            };

            _context.Enrollments.Add(enrollment);
            return await _context.SaveChangesAsync() > 0;
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
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
                return false;

            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.DroppedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Course>> GetAvailableCoursesAsync(Guid userId, Guid tenantId)
    {
        var enrolledCourseIds = await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Select(e => e.CourseId)
            .ToListAsync();

        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Where(c => c.TenantId == tenantId && !enrolledCourseIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentByUserAndCourseAsync(Guid userId, Guid courseId)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
    }
}