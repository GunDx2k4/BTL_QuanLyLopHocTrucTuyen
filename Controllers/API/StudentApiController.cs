using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Services;
using System.Security.Claims;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Api
{
    /// <summary>
    /// API Controller for AJAX requests from Student Module
    /// All responses return JSON with standard format
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/student")]
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

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private IActionResult ApiSuccess(string message, object? data = null)
        {
            return Ok(new
            {
                success = true,
                message,
                data
            });
        }

        private IActionResult ApiError(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message
            });
        }

        #endregion

        // ==========================================
        // COURSE ENROLLMENT ENDPOINTS
        // ==========================================

        /// <summary>
        /// POST /api/student/courses/enroll
        /// Enroll in a course via AJAX
        /// </summary>
        [HttpPost("courses/enroll")]
        public async Task<IActionResult> EnrollCourse([FromBody] EnrollCourseRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return ApiError("Unauthorized", 401);
                }

                if (request.CourseId == Guid.Empty)
                {
                    return ApiError("Course ID is required");
                }

                var success = await _enrollmentRepo.EnrollCourseAsync(userId, request.CourseId);

                if (success)
                {
                    _logger.LogInformation("User {UserId} enrolled in course {CourseId}",
                        userId, request.CourseId);

                    return ApiSuccess("Đăng ký khóa học thành công!");
                }
                else
                {
                    return ApiError("Bạn đã đăng ký khóa học này rồi!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in course");
                return ApiError("Có lỗi xảy ra khi đăng ký khóa học", 500);
            }
        }

        /// <summary>
        /// POST /api/student/courses/drop
        /// Drop a course via AJAX
        /// </summary>
        [HttpPost("courses/drop")]
        public async Task<IActionResult> DropCourse([FromBody] DropCourseRequest request)
        {
            try
            {
                if (request.EnrollmentId == Guid.Empty)
                {
                    return ApiError("Enrollment ID is required");
                }

                var success = await _enrollmentRepo.DropCourseAsync(request.EnrollmentId);

                if (success)
                {
                    _logger.LogInformation("User dropped enrollment {EnrollmentId}",
                        request.EnrollmentId);

                    return ApiSuccess("Hủy đăng ký khóa học thành công!");
                }
                else
                {
                    return ApiError("Không thể hủy đăng ký khóa học!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping course");
                return ApiError("Có lỗi xảy ra khi hủy đăng ký", 500);
            }
        }

        /// <summary>
        /// GET /api/student/courses/available
        /// Get available courses for enrollment
        /// </summary>
        [HttpGet("courses/available")]
        public async Task<IActionResult> GetAvailableCourses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userRepo.FindByIdAsync(userId);

                if (user?.TenantId == null)
                {
                    return ApiError("Bạn chưa thuộc tổ chức nào!");
                }

                var courses = await _enrollmentRepo
                    .GetAvailableCoursesAsync(userId, user.TenantId.Value);

                var courseDtos = courses.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.BeginTime,
                    c.EndTime,
                    c.Status,
                    InstructorName = c.Instructor?.FullName ?? "N/A",
                    EnrollmentCount = c.Enrollments?.Count ?? 0
                });

                return ApiSuccess("Success", courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available courses");
                return ApiError("Có lỗi xảy ra", 500);
            }
        }

        // ==========================================
        // ASSIGNMENT ENDPOINTS
        // ==========================================

        /// <summary>
        /// GET /api/student/assignments
        /// Get all assignments for current student
        /// </summary>
        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignments()
        {
            try
            {
                var userId = GetCurrentUserId();
                var assignments = await _assignmentRepo.GetAssignmentsByStudentIdAsync(userId);
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);

                var assignmentDtos = assignments.Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Description,
                    a.DueDate,
                    a.MaxScore,
                    CourseName = a.Lesson?.Course?.Name ?? "N/A",
                    InstructorName = a.Lesson?.Course?.Instructor?.FullName ?? "N/A",
                    IsOverdue = a.DueDate < DateTime.UtcNow,
                    TimeRemaining = a.DueDate - DateTime.UtcNow,
                    HasSubmitted = submissions.Any(s => s.AssignmentId == a.Id),
                    Submission = submissions.FirstOrDefault(s => s.AssignmentId == a.Id)
                });

                return ApiSuccess("Success", assignmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignments");
                return ApiError("Có lỗi xảy ra", 500);
            }
        }

        /// <summary>
        /// GET /api/student/assignments/{id}
        /// Get assignment details
        /// </summary>
        [HttpGet("assignments/{id}")]
        public async Task<IActionResult> GetAssignment(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var assignment = await _assignmentRepo.GetAssignmentByIdAsync(id);

                if (assignment == null)
                {
                    return ApiError("Không tìm thấy bài tập!", 404);
                }

                var submission = await _submissionRepo
                    .GetSubmissionByStudentAndAssignmentAsync(userId, id);

                var assignmentDto = new
                {
                    assignment.Id,
                    assignment.Title,
                    assignment.Description,
                    assignment.DueDate,
                    assignment.MaxScore,
                    assignment.Type,
                    CourseName = assignment.Lesson?.Course?.Name ?? "N/A",
                    InstructorName = assignment.Lesson?.Course?.Instructor?.FullName ?? "N/A",
                    IsOverdue = assignment.DueDate < DateTime.UtcNow,
                    TimeRemaining = assignment.DueDate - DateTime.UtcNow,
                    HasSubmitted = submission != null,
                    Submission = submission != null ? new
                    {
                        submission.Id,
                        submission.SubmittedAt,
                        submission.Grade,
                        submission.FileUrl
                    } : null
                };

                return ApiSuccess("Success", assignmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignment details");
                return ApiError("Có lỗi xảy ra", 500);
            }
        }

        // ==========================================
        // SUBMISSION ENDPOINTS
        // ==========================================

        /// <summary>
        /// POST /api/student/submissions/submit
        /// Submit assignment via AJAX with file upload
        /// </summary>
        [HttpPost("submissions/submit")]
        [RequestSizeLimit(52428800)] // 50MB
        public async Task<IActionResult> SubmitAssignment([FromForm] SubmitAssignmentRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Validate file
                var validationError = _fileUploadService.ValidateFile(request.File);
                if (validationError != null)
                {
                    return ApiError(validationError);
                }

                // Upload file
                var fileUrl = await _fileUploadService
                    .UploadSubmissionAsync(request.File, userId, request.AssignmentId);

                // Check existing submission
                var existingSubmission = await _submissionRepo
                    .GetSubmissionByStudentAndAssignmentAsync(userId, request.AssignmentId);

                if (existingSubmission != null)
                {
                    // Resubmit - Delete old file
                    if (!string.IsNullOrEmpty(existingSubmission.FileUrl))
                    {
                        await _fileUploadService.DeleteFileAsync(existingSubmission.FileUrl);
                    }

                    existingSubmission.FileUrl = fileUrl;
                    existingSubmission.SubmittedAt = DateTime.UtcNow;

                    var updated = await _submissionRepo.UpdateSubmissionAsync(existingSubmission);

                    if (updated)
                    {
                        _logger.LogInformation("User {UserId} resubmitted assignment {AssignmentId}",
                            userId, request.AssignmentId);

                        return ApiSuccess("Nộp lại bài tập thành công!", new
                        {
                            submissionId = existingSubmission.Id,
                            submittedAt = existingSubmission.SubmittedAt,
                            fileUrl = existingSubmission.FileUrl
                        });
                    }
                    else
                    {
                        return ApiError("Có lỗi khi nộp lại bài tập!");
                    }
                }
                else
                {
                    // First submission
                    var submission = new Submission
                    {
                        AssignmentId = request.AssignmentId,
                        StudentId = userId,
                        FileUrl = fileUrl,
                        SubmittedAt = DateTime.UtcNow
                    };

                    var created = await _submissionRepo.CreateSubmissionAsync(submission);

                    _logger.LogInformation("User {UserId} submitted assignment {AssignmentId}",
                        userId, request.AssignmentId);

                    return ApiSuccess("Nộp bài tập thành công!", new
                    {
                        submissionId = created.Id,
                        submittedAt = created.SubmittedAt,
                        fileUrl = created.FileUrl
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assignment");
                return ApiError("Có lỗi xảy ra khi nộp bài tập", 500);
            }
        }

        /// <summary>
        /// GET /api/student/submissions
        /// Get all submissions for current student
        /// </summary>
        [HttpGet("submissions")]
        public async Task<IActionResult> GetSubmissions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);

                var submissionDtos = submissions.Select(s => new
                {
                    s.Id,
                    s.SubmittedAt,
                    s.Grade,
                    s.FileUrl,
                    AssignmentTitle = s.Assignment?.Title ?? "N/A",
                    CourseName = s.Assignment?.Lesson?.Course?.Name ?? "N/A",
                    IsGraded = s.Grade.HasValue
                });

                return ApiSuccess("Success", submissionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submissions");
                return ApiError("Có lỗi xảy ra", 500);
            }
        }

        /// <summary>
        /// DELETE /api/student/submissions/{id}
        /// Delete a submission
        /// </summary>
        [HttpDelete("submissions/{id}")]
        public async Task<IActionResult> DeleteSubmission(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var submission = await _submissionRepo.GetSubmissionByIdAsync(id);

                if (submission == null)
                {
                    return ApiError("Không tìm thấy bài nộp!", 404);
                }

                if (submission.StudentId != userId)
                {
                    return ApiError("Bạn không có quyền xóa bài nộp này!", 403);
                }

                // Delete file first
                if (!string.IsNullOrEmpty(submission.FileUrl))
                {
                    await _fileUploadService.DeleteFileAsync(submission.FileUrl);
                }

                var success = await _submissionRepo.DeleteSubmissionAsync(id);

                if (success)
                {
                    return ApiSuccess("Xóa bài nộp thành công!");
                }
                else
                {
                    return ApiError("Không thể xóa bài nộp!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting submission");
                return ApiError("Có lỗi xảy ra", 500);
            }
        }

        // ==========================================
        // STATISTICS ENDPOINTS
        // ==========================================

        /// <summary>
        /// GET /api/student/dashboard
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var userId = GetCurrentUserId();

                var enrollments = await _enrollmentRepo.GetEnrollmentsByUserIdAsync(userId);
                var assignments = await _assignmentRepo.GetAssignmentsByStudentIdAsync(userId);
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);
                var upcomingAssignments = await _assignmentRepo.GetUpcomingAssignmentsAsync(userId, 7);
                var avgGrade = await _submissionRepo.GetAverageGradeAsync(userId);

                var pendingCount = assignments.Count(a =>
                    !submissions.Any(s => s.AssignmentId == a.Id));

                var stats = new
                {
                    TotalCourses = enrollments.Count(e =>
                        e.Status == Models.Enums.EnrollmentStatus.Enrolled),
                    PendingAssignments = pendingCount,
                    TotalSubmissions = submissions.Count,
                    AverageGrade = Math.Round(avgGrade, 1),
                    UpcomingAssignments = upcomingAssignments.Select(a => new
                    {
                        a.Id,
                        a.Title,
                        a.DueDate,
                        CourseName = a.Lesson?.Course?.Name ?? "N/A"
                    })
                };

                return ApiSuccess("Success", stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return ApiError("Có lỗi xảy ra", 500);
            }
        }

        /// <summary>
        /// GET /api/student/grades
        /// Get grade summary
        /// </summary>
        [HttpGet("grades")]
        public async Task<IActionResult> GetGrades()
        {
            try
            {
                var userId = GetCurrentUserId();
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);

                var gradedSubmissions = submissions.Where(s => s.Grade.HasValue).ToList();

                var gradeStats = new
                {
                    TotalSubmissions = submissions.Count,
                    GradedCount = gradedSubmissions.Count,
                    PendingCount = submissions.Count - gradedSubmissions.Count,
                    AverageGrade = gradedSubmissions.Any()
                        ? Math.Round(gradedSubmissions.Average(s => s.Grade!.Value), 1)
                        : 0,
                    MaxGrade = gradedSubmissions.Any()
                        ? Math.Round(gradedSubmissions.Max(s => s.Grade!.Value), 1)
                        : 0,
                    MinGrade = gradedSubmissions.Any()
                        ? Math.Round(gradedSubmissions.Min(s => s.Grade!.Value), 1)
                        : 0,
                    Submissions = gradedSubmissions.Select(s => new
                    {
                        s.Id,
                        s.SubmittedAt,
                        s.Grade,
                        AssignmentTitle = s.Assignment?.Title ?? "N/A",
                        CourseName = s.Assignment?.Lesson?.Course?.Name ?? "N/A"
                    })
                };

                return ApiSuccess("Success", gradeStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grades");
                return ApiError("Có lỗi xảy ra", 500);
            }
        }
    }

    #region Request DTOs

    public class EnrollCourseRequest
    {
        public Guid CourseId { get; set; }
    }

    public class DropCourseRequest
    {
        public Guid EnrollmentId { get; set; }
    }

    public class SubmitAssignmentRequest
    {
        public Guid AssignmentId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string? Notes { get; set; }
    }

    #endregion
}