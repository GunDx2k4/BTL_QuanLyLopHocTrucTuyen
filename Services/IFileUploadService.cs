namespace BTL_QuanLyLopHocTrucTuyen.Services
{
    /// <summary>
    /// Service xử lý upload và quản lý file
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Upload file submission lên server
        /// </summary>
        Task<string> UploadSubmissionAsync(IFormFile file, Guid studentId, Guid assignmentId);

        /// <summary>
        /// Upload file material lên server
        /// </summary>
        Task<string> UploadMaterialAsync(IFormFile file, Guid lessonId);

        /// <summary>
        /// Xóa file khỏi server
        /// </summary>
        Task<bool> DeleteFileAsync(string fileUrl);

        /// <summary>
        /// Lấy đường dẫn vật lý của file
        /// </summary>
        string GetPhysicalPath(string fileUrl);

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        bool FileExists(string fileUrl);

        /// <summary>
        /// Validate file upload
        /// </summary>
        string? ValidateFile(IFormFile file, int maxSizeMB = 50, string[]? allowedExtensions = null);

        /// <summary>
        /// Lấy tên file từ URL
        /// </summary>
        /// <param name="fileUrl">URL của file</param>
        /// <returns>Tên file</returns>
        string GetFileName(string fileUrl);  // ✅ ADDED

        /// <summary>
        /// Lấy thông tin file
        /// </summary>
        /// <param name="fileUrl">URL của file</param>
        /// <returns>FileInfo object</returns>
        FileInfo GetFileInfo(string fileUrl);  // ✅ ADDED

        /// <summary>
        /// Lấy kích thước file (bytes)
        /// </summary>
        /// <param name="fileUrl">URL của file</param>
        /// <returns>Kích thước file</returns>
        long GetFileSize(string fileUrl);  // ✅ ADDED
    }
}