using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql
{
    public class MySqlSubmissionRepository : ISubmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public MySqlSubmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Submission>> GetSubmissionsByStudentIdAsync(Guid studentId)
        {
            return await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                            .ThenInclude(c => c.Instructor)
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Submission?> GetSubmissionByIdAsync(Guid submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                            .ThenInclude(c => c.Instructor)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<List<Submission>> GetSubmissionsByAssignmentIdAsync(Guid assignmentId)
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Submission> CreateSubmissionAsync(Submission submission)
        {
            submission.SubmittedAt = DateTime.UtcNow;
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<bool> UpdateSubmissionAsync(Submission submission)
        {
            try
            {
                _context.Submissions.Update(submission);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSubmissionAsync(Guid submissionId)
        {
            try
            {
                var submission = await _context.Submissions.FindAsync(submissionId);
                if (submission == null)
                    return false;

                _context.Submissions.Remove(submission);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasSubmittedAsync(Guid studentId, Guid assignmentId)
        {
            return await _context.Submissions
                .AnyAsync(s => s.StudentId == studentId && s.AssignmentId == assignmentId);
        }

        public async Task<Submission?> GetSubmissionByStudentAndAssignmentAsync(Guid studentId, Guid assignmentId)
        {
            return await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.AssignmentId == assignmentId);
        }

        public async Task<double> GetAverageGradeAsync(Guid studentId)
        {
            var submissions = await _context.Submissions
                .Where(s => s.StudentId == studentId && s.Grade.HasValue)
                .ToListAsync();

            if (!submissions.Any())
                return 0;

            return submissions.Average(s => s.Grade!.Value);
        }
    }
}