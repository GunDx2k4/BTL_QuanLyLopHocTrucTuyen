using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Services;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using System.Security.Claims;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    /// <summary>
    /// Controller xử lý download file
    /// Vì file lưu ngoài wwwroot nên cần controller để serve
    /// </summary>
    [Authorize]
    public class FileController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly ILogger<FileController> _logger;

        public FileController(
            IFileUploadService fileUploadService,
            ISubmissionRepository submissionRepo,
            ILogger<FileController> logger)
        {
            _fileUploadService = fileUploadService;
            _submissionRepo = submissionRepo;
            _logger = logger;
        }

        /// <summary>
        /// Download file submission
        /// URL: /File/Download/Submissions/{filename}
        /// </summary>
        [HttpGet("File/Download/Submissions/{fileName}")]
        public async Task<IActionResult> DownloadSubmission(string fileName)
        {
            try
            {
                // Tạo đường dẫn tương đối
                var fileUrl = $"Uploads/Submissions/{fileName}";

                // Kiểm tra file tồn tại
                if (!_fileUploadService.FileExists(fileUrl))
                {
                    _logger.LogWarning($"File not found: {fileUrl}");
                    return NotFound("File không tồn tại!");
                }

                // Lấy đường dẫn vật lý
                var physicalPath = _fileUploadService.GetPhysicalPath(fileUrl);

                // Đọc file
                var memory = new MemoryStream();
                using (var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                // Xác định content type
                var contentType = GetContentType(fileName);

                // Trả về file
                return File(memory, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file: {fileName}");
                return StatusCode(500, "Có lỗi khi tải file!");
            }
        }

        /// <summary>
        /// Download file material
        /// URL: /File/Download/Materials/{filename}
        /// </summary>
        [HttpGet("File/Download/Materials/{fileName}")]
        public async Task<IActionResult> DownloadMaterial(string fileName)
        {
            try
            {
                var fileUrl = $"Uploads/Materials/{fileName}";

                if (!_fileUploadService.FileExists(fileUrl))
                {
                    _logger.LogWarning($"File not found: {fileUrl}");
                    return NotFound("File không tồn tại!");
                }

                var physicalPath = _fileUploadService.GetPhysicalPath(fileUrl);

                var memory = new MemoryStream();
                using (var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var contentType = GetContentType(fileName);

                return File(memory, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading material: {fileName}");
                return StatusCode(500, "Có lỗi khi tải file!");
            }
        }

        /// <summary>
        /// Download file submission theo ID (có kiểm tra quyền)
        /// URL: /File/DownloadSubmission/{submissionId}
        /// </summary>
        [HttpGet("File/DownloadSubmission/{submissionId}")]
        public async Task<IActionResult> DownloadSubmissionById(Guid submissionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var submission = await _submissionRepo.GetSubmissionByIdAsync(submissionId);

                if (submission == null)
                {
                    return NotFound("Không tìm thấy bài nộp!");
                }

                // Kiểm tra quyền: chỉ student nộp bài hoặc instructor của khóa học mới được tải
                var isStudent = submission.StudentId == userId;
                var isInstructor = submission.Assignment?.Lesson?.Course?.InstructorId == userId;

                if (!isStudent && !isInstructor)
                {
                    return Forbid();
                }

                if (string.IsNullOrEmpty(submission.FileUrl))
                {
                    return NotFound("File không tồn tại!");
                }

                if (!_fileUploadService.FileExists(submission.FileUrl))
                {
                    return NotFound("File không tồn tại trên server!");
                }

                var physicalPath = _fileUploadService.GetPhysicalPath(submission.FileUrl);

                var memory = new MemoryStream();
                using (var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var fileName = _fileUploadService.GetFileName(submission.FileUrl);
                var contentType = GetContentType(fileName);

                return File(memory, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading submission: {submissionId}");
                return StatusCode(500, "Có lỗi khi tải file!");
            }
        }

        /// <summary>
        /// Xác định Content-Type dựa vào extension
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Helper: Lấy UserId từ Claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}