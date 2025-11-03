using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories
{
    public interface IAssignmentRepository : IEntityRepository<Assignment>
    {

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