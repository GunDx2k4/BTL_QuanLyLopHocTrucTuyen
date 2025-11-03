using System;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql
{
    public class MySqlAssignmentRepository(MySqlDbContext context) : IAssignmentRepository
    {
        protected readonly DbSet<Assignment> _dbSet = context.Assignments;
        protected readonly IQueryable<Assignment> _queryable = context.Assignments
            .Include(a => a.Lesson)
            .Include(a => a.Submissions);

        public async Task<Assignment?> AddAsync(Assignment entity)
        {
            var result = await _dbSet.AddAsync(entity);
            await context.SaveChangesAsync();

            await context.Entry(entity).Reference(a => a.Lesson).LoadAsync();
            await context.Entry(entity).Collection(a => a.Submissions).LoadAsync();

            return result.Entity;
        }

        public async Task<IEnumerable<Assignment>> FindAsync()
        {
            return await _queryable.ToListAsync();
        }

        public async Task<Assignment?> FindByIdAsync(Guid id)
        {
            return await _queryable.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> DeleteAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            _dbSet.RemoveRange(entities);
            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteByIdAsync(Guid id)
        {
            var entity = await _queryable.FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null) return 0;
            _dbSet.Remove(entity);
            return await context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Assignment entity)
        {
            var existing = await _dbSet.FindAsync(entity.Id);
            if (existing == null) return 0;

            context.Entry(existing).CurrentValues.SetValues(entity);
            _dbSet.Update(existing);
            var updated = await context.SaveChangesAsync();

            await context.Entry(existing).Reference(a => a.Lesson).LoadAsync();
            await context.Entry(existing).Collection(a => a.Submissions).LoadAsync();

            return updated;
        }

        // ===== STUDENT-SPECIFIC METHODS =====

        public async Task<List<Assignment>> GetAssignmentsByCourseIdAsync(Guid courseId)
        {
            return await context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                .Include(a => a.Submissions)
                .Where(a => a.Lesson.CourseId == courseId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(Guid assignmentId)
        {
            return await context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
        }

        public async Task<List<Assignment>> GetAssignmentsByStudentIdAsync(Guid studentId)
        {
            // Lấy danh sách courseId mà student đã đăng ký
            var enrolledCourseIds = await context.Enrollments
                .Where(e => e.UserId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            // Lấy tất cả assignments của các khóa học đó
            return await context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .Where(a => a.Lesson.CourseId.HasValue && enrolledCourseIds.Contains(a.Lesson.CourseId.Value))
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<Assignment> CreateAssignmentAsync(Assignment assignment)
        {
            context.Assignments.Add(assignment);
            await context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> UpdateAssignmentAsync(Assignment assignment)
        {
            try
            {
                context.Assignments.Update(assignment);
                return await context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAssignmentAsync(Guid assignmentId)
        {
            try
            {
                var assignment = await context.Assignments.FindAsync(assignmentId);
                if (assignment == null)
                    return false;

                context.Assignments.Remove(assignment);
                return await context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Assignment>> GetUpcomingAssignmentsAsync(Guid studentId, int daysAhead = 7)
        {
            var enrolledCourseIds = await context.Enrollments
                .Where(e => e.UserId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var futureDate = now.AddDays(daysAhead);

            return await context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .Where(a => a.Lesson.CourseId.HasValue && enrolledCourseIds.Contains(a.Lesson.CourseId.Value)
                    && a.DueDate >= now
                    && a.DueDate <= futureDate
                    && !a.Submissions.Any(s => s.StudentId == studentId))
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetOverdueAssignmentsAsync(Guid studentId)
        {
            var enrolledCourseIds = await context.Enrollments
                .Where(e => e.UserId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return await context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .Where(a => a.Lesson.CourseId.HasValue && enrolledCourseIds.Contains(a.Lesson.CourseId.Value)
                    && a.DueDate < now
                    && !a.Submissions.Any(s => s.StudentId == studentId))
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
    }


}