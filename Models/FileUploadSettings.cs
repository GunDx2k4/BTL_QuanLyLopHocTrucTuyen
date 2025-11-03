namespace BTL_QuanLyLopHocTrucTuyen.Models
{
    /// <summary>
    /// Cấu hình cho việc upload file
    /// Đọc từ appsettings.json
    /// </summary>
    public class FileUploadSettings
    {
        /// <summary>
        /// Đường dẫn gốc lưu file (VD: "Uploads")
        /// </summary>
        public string UploadBasePath { get; set; } = "Uploads";

        /// <summary>
        /// Thư mục con lưu bài nộp (VD: "Submissions")
        /// </summary>
        public string SubmissionsPath { get; set; } = "Submissions";

        /// <summary>
        /// Thư mục con lưu tài liệu (VD: "Materials")
        /// </summary>
        public string MaterialsPath { get; set; } = "Materials";

        /// <summary>
        /// Kích thước file tối đa (MB)
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 50;

        /// <summary>
        /// Các định dạng file được phép
        /// </summary>
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Lấy đường dẫn đầy đủ đến thư mục Submissions
        /// </summary>
        public string GetSubmissionsFullPath(string basePath)
        {
            return Path.Combine(basePath, UploadBasePath, SubmissionsPath);
        }

        /// <summary>
        /// Lấy đường dẫn đầy đủ đến thư mục Materials
        /// </summary>
        public string GetMaterialsFullPath(string basePath)
        {
            return Path.Combine(basePath, UploadBasePath, MaterialsPath);
        }
    }
}