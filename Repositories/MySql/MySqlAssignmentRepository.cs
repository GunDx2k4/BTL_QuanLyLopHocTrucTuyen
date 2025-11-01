using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql
{
    public class MySqlAssignmentRepository : IAssignmentRepository
    {
        private readonly ApplicationDbContext _context;

        public MySqlAssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== GENERIC CRUD METHODS (FOR INSTRUCTOR) =====

        public async Task<IEnumerable<Assignment>> FindAsync()
        {
            return await _context.Assignments
                .Include(a => a.Lesson)
                .Include(a => a.Submissions)
                .ToListAsync();
        }

        public async Task<Assignment?> FindByIdAsync(Guid id)
        {
            return await _context.Assignments
                .Include(a => a.Lesson)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Assignment?> AddAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(assignment).Reference(a => a.Lesson).LoadAsync();
            await _context.Entry(assignment).Collection(a => a.Submissions).LoadAsync();

            return assignment;
        }

        public async Task<int> UpdateAsync(Assignment assignment)
        {
            _context.Assignments.Update(assignment);
            var result = await _context.SaveChangesAsync();

            // Reload navigation properties
            await _context.Entry(assignment).Reference(a => a.Lesson).LoadAsync();
            await _context.Entry(assignment).Collection(a => a.Submissions).LoadAsync();

            return result;
        }

        public async Task<int> DeleteByIdAsync(Guid id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return 0;

            _context.Assignments.Remove(assignment);
            return await _context.SaveChangesAsync();
        }

        // ===== STUDENT-SPECIFIC METHODS =====

        public async Task<List<Assignment>> GetAssignmentsByCourseIdAsync(Guid courseId)
        {
            return await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                .Include(a => a.Submissions)
                .Where(a => a.Lesson.CourseId == courseId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(Guid assignmentId)
        {
            return await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
        }

        public async Task<List<Assignment>> GetAssignmentsByStudentIdAsync(Guid studentId)
        {
            // Lấy danh sách courseId mà student đã đăng ký
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            // Lấy tất cả assignments của các khóa học đó
            return await _context.Assignments
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
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> UpdateAssignmentAsync(Assignment assignment)
        {
            try
            {
                _context.Assignments.Update(assignment);
                return await _context.SaveChangesAsync() > 0;
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
                var assignment = await _context.Assignments.FindAsync(assignmentId);
                if (assignment == null)
                    return false;

                _context.Assignments.Remove(assignment);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Assignment>> GetUpcomingAssignmentsAsync(Guid studentId, int daysAhead = 7)
        {
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var futureDate = now.AddDays(daysAhead);

            return await _context.Assignments
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
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return await _context.Assignments
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