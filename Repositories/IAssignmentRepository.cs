using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories
{
    public interface IAssignmentRepository
    {
        // ===== METHODS FOR INSTRUCTOR =====
        Task<IEnumerable<Assignment>> FindAsync();
        Task<Assignment?> FindByIdAsync(Guid id);
        Task<Assignment?> AddAsync(Assignment assignment);
        Task<int> UpdateAsync(Assignment assignment);
        Task<int> DeleteByIdAsync(Guid id);

        // ===== METHODS FOR STUDENT =====
        Task<List<Assignment>> GetAssignmentsByCourseIdAsync(Guid courseId);
        Task<Assignment?> GetAssignmentByIdAsync(Guid assignmentId);
        Task<List<Assignment>> GetAssignmentsByStudentIdAsync(Guid studentId);
        Task<Assignment> CreateAssignmentAsync(Assignment assignment);
        Task<bool> UpdateAssignmentAsync(Assignment assignment);
        Task<bool> DeleteAssignmentAsync(Guid assignmentId);
        Task<List<Assignment>> GetUpcomingAssignmentsAsync(Guid studentId, int daysAhead = 7);
        Task<List<Assignment>> GetOverdueAssignmentsAsync(Guid studentId);
    }
}