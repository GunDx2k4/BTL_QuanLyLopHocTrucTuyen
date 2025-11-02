
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories
{
    /// <summary>
    /// Repository xử lý các thao tác liên quan đến Submission (Bài nộp)
    /// </summary>
    public interface ISubmissionRepository : IEntityRepository<Submission>
    {
        /// <summary>
        /// Lấy tất cả bài nộp của một student
        /// </summary>
        Task<List<Submission>> GetSubmissionsByStudentIdAsync(Guid studentId);

        /// <summary>
        /// Lấy chi tiết một bài nộp
        /// </summary>
        Task<Submission?> GetSubmissionByIdAsync(Guid submissionId);

        /// <summary>
        /// Lấy tất cả bài nộp của một assignment cụ thể
        /// </summary>
        Task<List<Submission>> GetSubmissionsByAssignmentIdAsync(Guid assignmentId);

        /// <summary>
        /// Tạo bài nộp mới
        /// </summary>
        Task<Submission> CreateSubmissionAsync(Submission submission);

        /// <summary>
        /// Cập nhật bài nộp (nộp lại)
        /// </summary>
        Task<bool> UpdateSubmissionAsync(Submission submission);

        /// <summary>
        /// Xóa bài nộp
        /// </summary>
        Task<bool> DeleteSubmissionAsync(Guid submissionId);

        /// <summary>
        /// Kiểm tra student đã nộp bài cho assignment này chưa
        /// </summary>
        Task<bool> HasSubmittedAsync(Guid studentId, Guid assignmentId);

        /// <summary>
        /// Lấy bài nộp của student cho một assignment cụ thể
        /// </summary>
        Task<Submission?> GetSubmissionByStudentAndAssignmentAsync(Guid studentId, Guid assignmentId);

        /// <summary>
        /// Lấy điểm trung bình của student
        /// </summary>
        Task<double> GetAverageGradeAsync(Guid studentId);
    }
}
