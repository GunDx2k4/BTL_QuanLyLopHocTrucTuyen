using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Services;
using System.Security.Claims;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API
{
    [ApiController]
    [Route("api/student")]
    [Authorize]
    public class StudentApiController : ControllerBase
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<StudentApiController> _logger;

        public StudentApiController(
            IEnrollmentRepository enrollmentRepo,
            IAssignmentRepository assignmentRepo,
            ISubmissionRepository submissionRepo,
            ICourseRepository courseRepo,
            IUserRepository userRepo,
            IFileUploadService fileUploadService,
            ILogger<StudentApiController> logger)
        {
            _enrollmentRepo = enrollmentRepo;
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _courseRepo = courseRepo;
            _userRepo = userRepo;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        // Helper: Lấy UserId
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // ============================================
        // 1. ENROLL COURSE
        // POST: api/student/enroll
        // ============================================
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollCourse([FromBody] EnrollCourseRequest request)
        {
            try
            {
                if (request == null || request.CourseId == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { success = false, message = "Bạn cần đăng nhập!" });
                }

                var success = await _enrollmentRepo.EnrollCourseAsync(userId, request.CourseId);

                if (success)
                {
                    _logger.LogInformation($"User {userId} enrolled in course {request.CourseId}");
                    return Ok(new
                    {
                        success = true,
                        message = "Đăng ký khóa học thành công!"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bạn đã đăng ký khóa học này rồi!"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling course");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // ============================================
        // 2. GET AVAILABLE COURSES
        // GET: api/student/available-courses
        // ============================================
        [HttpGet("available-courses")]
        public async Task<IActionResult> GetAvailableCourses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userRepo.FindByIdAsync(userId);

                if (user?.TenantId == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bạn chưa thuộc tổ chức nào!"
                    });
                }

                var courses = await _enrollmentRepo.GetAvailableCoursesAsync(userId, user.TenantId.Value);

                return Ok(new
                {
                    success = true,
                    total = courses.Count,
                    data = courses.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        description = c.Description,
                        instructor = c.Instructor?.FullName,
                        beginTime = c.BeginTime,
                        endTime = c.EndTime,
                        status = c.Status.ToString(),
                        enrollmentCount = c.Enrollments?.Count ?? 0
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available courses");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra!"
                });
            }
        }

        // ============================================
        // 3. DROP COURSE
        // POST: api/student/drop-course/{enrollmentId}
        // ============================================
        [HttpPost("drop-course/{enrollmentId}")]
        public async Task<IActionResult> DropCourse(Guid enrollmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var enrollment = await _enrollmentRepo.GetEnrollmentByIdAsync(enrollmentId);

                if (enrollment == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đăng ký khóa học!"
                    });
                }

                // Kiểm tra quyền
                if (enrollment.UserId != userId)
                {
                    return Forbid();
                }

                var success = await _enrollmentRepo.DropCourseAsync(enrollmentId);

                if (success)
                {
                    _logger.LogInformation($"User {userId} dropped enrollment {enrollmentId}");
                    return Ok(new
                    {
                        success = true,
                        message = "Hủy đăng ký khóa học thành công!"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thể hủy đăng ký!"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping course");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra!"
                });
            }
        }

        // ============================================
        // 4. DOWNLOAD SUBMISSION FILE
        // GET: api/student/submissions/{id}/download
        // ============================================
        [HttpGet("submissions/{id}/download")]
        public async Task<IActionResult> DownloadSubmission(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var submission = await _submissionRepo.GetSubmissionByIdAsync(id);

                if (submission == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy bài nộp!" });
                }

                // Kiểm tra quyền
                if (submission.StudentId != userId)
                {
                    return Forbid();
                }

                if (string.IsNullOrEmpty(submission.FileUrl))
                {
                    return NotFound(new { success = false, message = "File không tồn tại!" });
                }

                var filePath = _fileUploadService.GetPhysicalPath(submission.FileUrl);

                if (!_fileUploadService.FileExists(submission.FileUrl))
                {
                    return NotFound(new { success = false, message = "File không tồn tại trên server!" });
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var fileName = Path.GetFileName(filePath);
                var contentType = "application/octet-stream";

                return File(memory, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading submission");
                return StatusCode(500, new { success = false, message = "Có lỗi khi tải file!" });
            }
        }

        // ============================================
        // 5. SUBMIT ASSIGNMENT (AJAX with file)
        // POST: api/student/submit-assignment
        // ============================================
        [HttpPost("submit-assignment")]
        public async Task<IActionResult> SubmitAssignmentAjax([FromForm] SubmitAssignmentRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (request.File == null || request.AssignmentId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ!"
                    });
                }

                // Validate file
                var validationError = _fileUploadService.ValidateFile(request.File);
                if (validationError != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = validationError
                    });
                }

                // Upload file
                var fileUrl = await _fileUploadService.UploadSubmissionAsync(
                    request.File,
                    userId,
                    request.AssignmentId
                );

                // Kiểm tra đã nộp chưa
                var existingSubmission = await _submissionRepo
                    .GetSubmissionByStudentAndAssignmentAsync(userId, request.AssignmentId);

                if (existingSubmission != null)
                {
                    // Nộp lại - Xóa file cũ
                    if (!string.IsNullOrEmpty(existingSubmission.FileUrl))
                    {
                        await _fileUploadService.DeleteFileAsync(existingSubmission.FileUrl);
                    }

                    existingSubmission.FileUrl = fileUrl;
                    existingSubmission.SubmittedAt = DateTime.UtcNow;

                    await _submissionRepo.UpdateSubmissionAsync(existingSubmission);

                    _logger.LogInformation($"User {userId} resubmitted assignment {request.AssignmentId}");

                    return Ok(new
                    {
                        success = true,
                        message = "Nộp lại bài tập thành công!",
                        submissionId = existingSubmission.Id
                    });
                }
                else
                {
                    // Nộp lần đầu
                    var submission = new BTL_QuanLyLopHocTrucTuyen.Models.Submission
                    {
                        AssignmentId = request.AssignmentId,
                        StudentId = userId,
                        FileUrl = fileUrl,
                        SubmittedAt = DateTime.UtcNow
                    };

                    var created = await _submissionRepo.CreateSubmissionAsync(submission);

                    _logger.LogInformation($"User {userId} submitted assignment {request.AssignmentId}");

                    return Ok(new
                    {
                        success = true,
                        message = "Nộp bài tập thành công!",
                        submissionId = created.Id
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assignment");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // ============================================
        // 6. GET STATISTICS
        // GET: api/student/statistics
        // ============================================
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var userId = GetCurrentUserId();

                var enrollments = await _enrollmentRepo.GetEnrollmentsByUserIdAsync(userId);
                var assignments = await _assignmentRepo.GetAssignmentsByStudentIdAsync(userId);
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);
                var avgGrade = await _submissionRepo.GetAverageGradeAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalCourses = enrollments.Count,
                        activeCourses = enrollments.Count(e => e.Status == BTL_QuanLyLopHocTrucTuyen.Models.Enums.EnrollmentStatus.Enrolled),
                        totalAssignments = assignments.Count,
                        pendingAssignments = assignments.Count(a => !submissions.Any(s => s.AssignmentId == a.Id)),
                        totalSubmissions = submissions.Count,
                        gradedSubmissions = submissions.Count(s => s.Grade.HasValue),
                        averageGrade = Math.Round(avgGrade, 1)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra!" });
            }
        }

        // ============================================
        // 7. EXPORT REPORT
        // GET: api/student/reports/export?format=pdf|excel
        // ============================================
        [HttpGet("reports/export")]
        public async Task<IActionResult> ExportReport([FromQuery] string format = "pdf")
        {
            try
            {
                var userId = GetCurrentUserId();

                // TODO: Implement PDF/Excel export
                // Tạm thời return thông báo

                return Ok(new
                {
                    success = false,
                    message = "Chức năng xuất báo cáo đang được phát triển!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra!" });
            }
        }
    }

    // ============================================
    // REQUEST MODELS
    // ============================================
    public class EnrollCourseRequest
    {
        public Guid CourseId { get; set; }
    }

    public class SubmitAssignmentRequest
    {
        public Guid AssignmentId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string? Notes { get; set; }
    }
}