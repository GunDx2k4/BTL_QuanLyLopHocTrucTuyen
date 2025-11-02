
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories.MySql
{
    public class MySqlSubmissionRepository(MySqlDbContext context) : ISubmissionRepository
    {
        protected readonly DbSet<Submission> _dbSet = context.Submissions;
        protected readonly IQueryable<Submission> _queryable = context.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student);

        public async Task<Submission?> AddAsync(Submission entity)
        {
            var result = await _dbSet.AddAsync(entity);
            await context.SaveChangesAsync();

            await context.Entry(entity).Reference(s => s.Assignment).LoadAsync();
            await context.Entry(entity).Reference(s => s.Student).LoadAsync();

            return result.Entity;
        }

        public async Task<IEnumerable<Submission>> FindAsync()
        {
            return await _queryable.ToListAsync();
        }

        public async Task<Submission?> FindByIdAsync(Guid id)
        {
            return await _queryable.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<int> UpdateAsync(Submission entity)
        {
            var existing = await _dbSet.FindAsync(entity.Id);
            if (existing == null) return 0;

            context.Entry(existing).CurrentValues.SetValues(entity);
            _dbSet.Update(existing);
            var updated = await context.SaveChangesAsync();

            await context.Entry(existing).Reference(s => s.Assignment).LoadAsync();
            await context.Entry(existing).Reference(s => s.Student).LoadAsync();

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
            var entity = await _queryable.FirstOrDefaultAsync(s => s.Id == id);
            if (entity is null) return 0;
            _dbSet.Remove(entity);
            return await context.SaveChangesAsync();
        }



        public async Task<List<Submission>> GetSubmissionsByStudentIdAsync(Guid studentId)
        {
            return await context.Submissions
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
            return await context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                            .ThenInclude(c => c.Instructor)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<List<Submission>> GetSubmissionsByAssignmentIdAsync(Guid assignmentId)
        {
            return await context.Submissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Submission> CreateSubmissionAsync(Submission submission)
        {
            submission.SubmittedAt = DateTime.UtcNow;
            context.Submissions.Add(submission);
            await context.SaveChangesAsync();
            return submission;
        }

        public async Task<bool> UpdateSubmissionAsync(Submission submission)
        {
            try
            {
                context.Submissions.Update(submission);
                return await context.SaveChangesAsync() > 0;
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
                var submission = await context.Submissions.FindAsync(submissionId);
                if (submission == null)
                    return false;

                context.Submissions.Remove(submission);
                return await context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasSubmittedAsync(Guid studentId, Guid assignmentId)
        {
            return await context.Submissions
                .AnyAsync(s => s.StudentId == studentId && s.AssignmentId == assignmentId);
        }

        public async Task<Submission?> GetSubmissionByStudentAndAssignmentAsync(Guid studentId, Guid assignmentId)
        {
            return await context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.AssignmentId == assignmentId);
        }

        public async Task<double> GetAverageGradeAsync(Guid studentId)
        {
            var submissions = await context.Submissions
                .Where(s => s.StudentId == studentId && s.Grade.HasValue)
                .ToListAsync();

            if (!submissions.Any())
                return 0;

            return submissions.Average(s => s.Grade!.Value);
        }
    }
}
