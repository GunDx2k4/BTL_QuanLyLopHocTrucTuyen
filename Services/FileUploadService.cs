using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.Extensions.Options;

namespace BTL_QuanLyLopHocTrucTuyen.Services
{

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly FileUploadSettings _settings;
        private readonly string _uploadBasePath;

        public FileUploadService(
            IWebHostEnvironment environment,
            ILogger<FileUploadService> logger,
            IOptions<FileUploadSettings> settings)
        {
            _environment = environment;
            _logger = logger;
            _settings = settings.Value;

            // Tạo đường dẫn tuyệt đối đến thư mục Uploads (ngoài wwwroot)
            // VD: C:\Projects\MyApp\Uploads\
            _uploadBasePath = Path.Combine(_environment.ContentRootPath, _settings.UploadBasePath);

            // Tạo thư mục nếu chưa tồn tại
            EnsureUploadDirectoriesExist();
        }

        /// <summary>
        /// Tạo các thư mục uploads nếu chưa tồn tại
        /// </summary>
        private void EnsureUploadDirectoriesExist()
        {
            try
            {
                var submissionsPath = Path.Combine(_uploadBasePath, _settings.SubmissionsPath);
                var materialsPath = Path.Combine(_uploadBasePath, _settings.MaterialsPath);

                if (!Directory.Exists(submissionsPath))
                {
                    Directory.CreateDirectory(submissionsPath);
                    _logger.LogInformation($"Created submissions directory: {submissionsPath}");
                }

                if (!Directory.Exists(materialsPath))
                {
                    Directory.CreateDirectory(materialsPath);
                    _logger.LogInformation($"Created materials directory: {materialsPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating upload directories");
                throw;
            }
        }

        /// <summary>
        /// Upload file submission
        /// </summary>
        public async Task<string> UploadSubmissionAsync(IFormFile file, Guid studentId, Guid assignmentId)
        {
            try
            {
                // Validate file
                var validationError = ValidateFile(file);
                if (validationError != null)
                {
                    throw new InvalidOperationException(validationError);
                }

                // Tạo tên file unique
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{studentId}_{assignmentId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

                // Đường dẫn vật lý đầy đủ
                var uploadPath = Path.Combine(_uploadBasePath, _settings.SubmissionsPath);
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded successfully: {fileName}");

                // Trả về đường dẫn tương đối (để lưu vào DB)
                // VD: "Uploads/Submissions/abc123.zip"
                return Path.Combine(_settings.UploadBasePath, _settings.SubmissionsPath, fileName)
                    .Replace("\\", "/"); // Chuẩn hóa path separator
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading submission file");
                throw new InvalidOperationException("Không thể upload file. Vui lòng thử lại!", ex);
            }
        }

        /// <summary>
        /// Upload file material
        /// </summary>
        public async Task<string> UploadMaterialAsync(IFormFile file, Guid lessonId)
        {
            try
            {
                // Validate file
                var validationError = ValidateFile(file);
                if (validationError != null)
                {
                    throw new InvalidOperationException(validationError);
                }

                // Tạo tên file unique
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{lessonId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

                // Đường dẫn vật lý đầy đủ
                var uploadPath = Path.Combine(_uploadBasePath, _settings.MaterialsPath);
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"Material file uploaded successfully: {fileName}");

                // Trả về đường dẫn tương đối
                return Path.Combine(_settings.UploadBasePath, _settings.MaterialsPath, fileName)
                    .Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading material file");
                throw new InvalidOperationException("Không thể upload tài liệu. Vui lòng thử lại!", ex);
            }
        }

        /// <summary>
        /// Xóa file
        /// </summary>
        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return false;

                var filePath = GetPhysicalPath(fileUrl);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation($"File deleted successfully: {fileUrl}");
                    return true;
                }

                _logger.LogWarning($"File not found for deletion: {fileUrl}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {fileUrl}");
                return false;
            }
        }

        /// <summary>
        /// Lấy đường dẫn vật lý của file
        /// </summary>
        public string GetPhysicalPath(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return string.Empty;

            // Chuẩn hóa path separator
            var normalizedPath = fileUrl.Replace("/", Path.DirectorySeparatorChar.ToString());

            // Tạo đường dẫn đầy đủ
            // VD: C:\Projects\MyApp\Uploads\Submissions\abc123.zip
            return Path.Combine(_environment.ContentRootPath, normalizedPath);
        }

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        public bool FileExists(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return false;

            var filePath = GetPhysicalPath(fileUrl);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Validate file upload
        /// </summary>
        public string? ValidateFile(IFormFile file, int maxSizeMB = 0, string[]? allowedExtensions = null)
        {
            // Sử dụng config từ appsettings nếu không truyền vào
            maxSizeMB = maxSizeMB > 0 ? maxSizeMB : _settings.MaxFileSizeMB;
            allowedExtensions = allowedExtensions ?? _settings.AllowedExtensions;

            // Kiểm tra file null
            if (file == null || file.Length == 0)
            {
                return "Vui lòng chọn file để upload!";
            }

            // Kiểm tra kích thước
            var maxSizeBytes = maxSizeMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                return $"File quá lớn! Kích thước tối đa là {maxSizeMB}MB.";
            }

            // Kiểm tra extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return $"Định dạng file không được phép! Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}";
            }

            // Kiểm tra tên file
            if (string.IsNullOrWhiteSpace(file.FileName) || file.FileName.Length > 255)
            {
                return "Tên file không hợp lệ!";
            }

            // Kiểm tra ký tự đặc biệt nguy hiểm trong tên file
            var invalidChars = Path.GetInvalidFileNameChars();
            if (file.FileName.Any(c => invalidChars.Contains(c)))
            {
                return "Tên file chứa ký tự không hợp lệ!";
            }

            return null; // Valid
        }

        /// <summary>
        /// Lấy thông tin file
        /// </summary>
        public FileInfo GetFileInfo(string fileUrl)
        {
            var physicalPath = GetPhysicalPath(fileUrl);
            return new FileInfo(physicalPath);
        }

        /// <summary>
        /// Lấy kích thước file (bytes)
        /// </summary>
        public long GetFileSize(string fileUrl)
        {
            var fileInfo = GetFileInfo(fileUrl);
            return fileInfo.Exists ? fileInfo.Length : 0;
        }

        /// <summary>
        /// Lấy tên file từ URL
        /// </summary>
        public string GetFileName(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return string.Empty;

            return Path.GetFileName(fileUrl);
        }
    }
}