using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Repositories
{
    /// <summary>
    /// Repository xử lý các thao tác liên quan đến Enrollment (Đăng ký khóa học)
    /// </summary>
    public interface IEnrollmentRepository
    {
        /// <summary>
        /// Lấy danh sách tất cả enrollment của một user
        /// </summary>
        Task<List<Enrollment>> GetEnrollmentsByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy thông tin chi tiết một enrollment
        /// </summary>
        Task<Enrollment?> GetEnrollmentByIdAsync(Guid enrollmentId);

        /// <summary>
        /// Đăng ký khóa học mới cho user
        /// </summary>
        Task<bool> EnrollCourseAsync(Guid userId, Guid courseId);

        /// <summary>
        /// Hủy đăng ký khóa học
        /// </summary>
        Task<bool> DropCourseAsync(Guid enrollmentId);

        /// <summary>
        /// Lấy danh sách khóa học có thể đăng ký (chưa đăng ký)
        /// </summary>
        Task<List<Course>> GetAvailableCoursesAsync(Guid userId, Guid tenantId);

        /// <summary>
        /// Lấy enrollment theo userId và courseId
        /// </summary>
        Task<Enrollment?> GetEnrollmentByUserAndCourseAsync(Guid userId, Guid courseId);
    }
}