using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories
{
    /// <summary>
    /// Repository xử lý các thao tác liên quan đến Assignment (Bài tập)
    /// </summary>
    public interface IAssignmentRepository
    {
        /// <summary>
        /// Lấy tất cả assignment của một khóa học
        /// </summary>
        Task<List<Assignment>> GetAssignmentsByCourseIdAsync(Guid courseId);

        /// <summary>
        /// Lấy chi tiết một assignment
        /// </summary>
        Task<Assignment?> GetAssignmentByIdAsync(Guid assignmentId);

        /// <summary>
        /// Lấy tất cả assignment của các khóa học mà student đã đăng ký
        /// </summary>
        Task<List<Assignment>> GetAssignmentsByStudentIdAsync(Guid studentId);

        /// <summary>
        /// Tạo assignment mới
        /// </summary>
        Task<Assignment> CreateAssignmentAsync(Assignment assignment);

        /// <summary>
        /// Cập nhật assignment
        /// </summary>
        Task<bool> UpdateAssignmentAsync(Assignment assignment);

        /// <summary>
        /// Xóa assignment
        /// </summary>
        Task<bool> DeleteAssignmentAsync(Guid assignmentId);

        /// <summary>
        /// Lấy danh sách assignment sắp đến hạn (trong vòng X ngày)
        /// </summary>
        Task<List<Assignment>> GetUpcomingAssignmentsAsync(Guid studentId, int daysAhead = 7);

        /// <summary>
        /// Lấy danh sách assignment quá hạn của student
        /// </summary>
        Task<List<Assignment>> GetOverdueAssignmentsAsync(Guid studentId);
    }
}