using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Services;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng dành cho sinh viên
    /// </summary>
    [Authorize]
    [Route("Student")]
    public class StudentController : Controller
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IEnrollmentRepository enrollmentRepo,
            IAssignmentRepository assignmentRepo,
            ISubmissionRepository submissionRepo,
            ICourseRepository courseRepo,
            IUserRepository userRepo,
            IFileUploadService fileUploadService,
            ILogger<StudentController> logger)
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

        /// <summary>
        /// Lấy UserId từ Claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Lấy thông tin User hiện tại
        /// </summary>
        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return null;
            return await _userRepo.FindByIdAsync(userId);
        }

        /// <summary>
        /// Kiểm tra user có quyền truy cập submission không
        /// </summary>
        private async Task<bool> CanAccessSubmissionAsync(Guid submissionId)
        {
            var userId = GetCurrentUserId();
            var submission = await _submissionRepo.GetSubmissionByIdAsync(submissionId);
            return submission?.StudentId == userId;
        }

        #endregion

        // ============================================
        // 1. DASHBOARD
        // ============================================
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthorized access attempt to Student dashboard");
                    return RedirectToAction("Login", "Home");
                }

                var enrollments = await _enrollmentRepo.GetEnrollmentsByUserIdAsync(userId);
                var assignments = await _assignmentRepo.GetAssignmentsByStudentIdAsync(userId);
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);
                var upcomingAssignments = await _assignmentRepo.GetUpcomingAssignmentsAsync(userId, 7);
                var avgGrade = await _submissionRepo.GetAverageGradeAsync(userId);

                var pendingCount = assignments.Count(a =>
                    !submissions.Any(s => s.AssignmentId == a.Id));

                ViewBag.TotalCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Enrolled);
                ViewBag.PendingAssignments = pendingCount;
                ViewBag.TotalSubmissions = submissions.Count;
                ViewBag.AverageGrade = Math.Round(avgGrade, 1);
                ViewBag.UpcomingAssignments = upcomingAssignments;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student dashboard");
                TempData["Error"] = "Có lỗi xảy ra khi tải dashboard.";
                return View();
            }
        }

        // ============================================
        // 2. MY COURSES
        // ============================================
        [HttpGet("MyCourses")]
        public async Task<IActionResult> MyCourses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var enrollments = await _enrollmentRepo.GetEnrollmentsByUserIdAsync(userId);

                ViewBag.ActiveCourses = enrollments
                    .Where(e => e.Status == EnrollmentStatus.Enrolled)
                    .ToList();
                ViewBag.CompletedCourses = enrollments
                    .Where(e => e.Status == EnrollmentStatus.Completed)
                    .ToList();
                ViewBag.DroppedCourses = enrollments
                    .Where(e => e.Status == EnrollmentStatus.Dropped)
                    .ToList();

                return View(enrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student courses");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách khóa học.";
                return View(new List<Enrollment>());
            }
        }

        // ============================================
        // 3. AVAILABLE COURSES
        // ============================================
        [HttpGet("AvailableCourses")]
        public async Task<IActionResult> AvailableCourses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await GetCurrentUserAsync();

                if (user?.TenantId == null)
                {
                    TempData["Error"] = "Bạn chưa thuộc tổ chức nào!";
                    return View(new List<Course>());
                }

                var availableCourses = await _enrollmentRepo
                    .GetAvailableCoursesAsync(userId, user.TenantId.Value);

                ViewBag.TotalAvailable = availableCourses.Count;
                return View(availableCourses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available courses");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách khóa học.";
                return View(new List<Course>());
            }
        }

        // ============================================
        // 4. ENROLL COURSE (MVC)
        // ============================================
        [HttpPost("EnrollCourse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(Guid courseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _enrollmentRepo.EnrollCourseAsync(userId, courseId);

                if (success)
                {
                    TempData["Success"] = "Đăng ký khóa học thành công!";
                    _logger.LogInformation("User {UserId} enrolled in course {CourseId}",
                        userId, courseId);
                    return RedirectToAction(nameof(MyCourses));
                }
                else
                {
                    TempData["Error"] = "Bạn đã đăng ký khóa học này rồi!";
                    return RedirectToAction(nameof(AvailableCourses));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in course {CourseId}", courseId);
                TempData["Error"] = "Có lỗi xảy ra khi đăng ký khóa học.";
                return RedirectToAction(nameof(AvailableCourses));
            }
        }

        // ============================================
        // 5. COURSE DETAILS
        // ============================================
        [HttpGet("CourseDetails/{id}")]
        public async Task<IActionResult> CourseDetails(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var enrollment = await _enrollmentRepo
                    .GetEnrollmentByUserAndCourseAsync(userId, id);

                if (enrollment == null)
                {
                    TempData["Error"] = "Bạn chưa đăng ký khóa học này!";
                    return RedirectToAction(nameof(MyCourses));
                }

                var assignments = await _assignmentRepo.GetAssignmentsByCourseIdAsync(id);
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);

                ViewBag.Enrollment = enrollment;
                ViewBag.Assignments = assignments;
                ViewBag.Submissions = submissions;

                return View(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading course details for course {CourseId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin khóa học.";
                return RedirectToAction(nameof(MyCourses));
            }
        }

        // ============================================
        // 6. DROP COURSE
        // ============================================
        [HttpPost("DropCourse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropCourse(Guid enrollmentId)
        {
            try
            {
                var success = await _enrollmentRepo.DropCourseAsync(enrollmentId);

                if (success)
                {
                    TempData["Success"] = "Hủy đăng ký khóa học thành công!";
                    _logger.LogInformation("User dropped enrollment {EnrollmentId}", enrollmentId);
                }
                else
                {
                    TempData["Error"] = "Không thể hủy đăng ký khóa học!";
                }

                return RedirectToAction(nameof(MyCourses));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping enrollment {EnrollmentId}", enrollmentId);
                TempData["Error"] = "Có lỗi xảy ra khi hủy đăng ký.";
                return RedirectToAction(nameof(MyCourses));
            }
        }

        // ============================================
        // 7. ASSIGNMENTS
        // ============================================
        [HttpGet("Assignments")]
        public async Task<IActionResult> Assignments()
        {
            try
            {
                var userId = GetCurrentUserId();
                var assignments = await _assignmentRepo.GetAssignmentsByStudentIdAsync(userId);
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);
                var upcomingAssignments = await _assignmentRepo.GetUpcomingAssignmentsAsync(userId);
                var overdueAssignments = await _assignmentRepo.GetOverdueAssignmentsAsync(userId);

                ViewBag.TotalAssignments = assignments.Count;
                ViewBag.UpcomingCount = upcomingAssignments.Count;
                ViewBag.SubmittedCount = submissions.Count;
                ViewBag.PendingCount = assignments.Count - submissions.Count;
                ViewBag.OverdueCount = overdueAssignments.Count;
                ViewBag.Submissions = submissions;
                ViewBag.UpcomingAssignments = upcomingAssignments;
                ViewBag.OverdueAssignments = overdueAssignments;

                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assignments");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách bài tập.";
                return View(new List<Assignment>());
            }
        }

        // ============================================
        // 8. SUBMIT ASSIGNMENT - GET
        // ============================================
        [HttpGet("SubmitAssignment/{id}")]
        public async Task<IActionResult> SubmitAssignment(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var assignment = await _assignmentRepo.GetAssignmentByIdAsync(id);

                if (assignment == null)
                {
                    TempData["Error"] = "Không tìm thấy bài tập!";
                    return RedirectToAction(nameof(Assignments));
                }

                var existingSubmission = await _submissionRepo
                    .GetSubmissionByStudentAndAssignmentAsync(userId, id);

                ViewBag.ExistingSubmission = existingSubmission;
                ViewBag.Assignment = assignment;

                // ✅ Use DueDate from fixed Assignment model
                var timeRemaining = assignment.DueDate - DateTime.UtcNow;
                ViewBag.TimeRemaining = timeRemaining;
                ViewBag.IsOverdue = timeRemaining.TotalSeconds < 0;

                return View(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assignment {AssignmentId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải bài tập.";
                return RedirectToAction(nameof(Assignments));
            }
        }

        // ============================================
        // 9. SUBMIT ASSIGNMENT - POST
        // ============================================
        [HttpPost("SubmitAssignment")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)] // 50MB
        public async Task<IActionResult> SubmitAssignment(
            Guid assignmentId,
            IFormFile file,
            string? notes)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Validate file
                var validationError = _fileUploadService.ValidateFile(file);
                if (validationError != null)
                {
                    TempData["Error"] = validationError;
                    return RedirectToAction(nameof(SubmitAssignment), new { id = assignmentId });
                }

                // Upload file
                var fileUrl = await _fileUploadService
                    .UploadSubmissionAsync(file, userId, assignmentId);

                // Check existing submission
                var existingSubmission = await _submissionRepo
                    .GetSubmissionByStudentAndAssignmentAsync(userId, assignmentId);

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
                        TempData["Success"] = "Nộp lại bài tập thành công!";
                        _logger.LogInformation("User {UserId} resubmitted assignment {AssignmentId}",
                            userId, assignmentId);
                    }
                    else
                    {
                        TempData["Error"] = "Có lỗi khi nộp lại bài tập!";
                    }
                }
                else
                {
                    // First submission
                    var submission = new Submission
                    {
                        AssignmentId = assignmentId,
                        StudentId = userId,
                        FileUrl = fileUrl,
                        SubmittedAt = DateTime.UtcNow
                    };

                    await _submissionRepo.CreateSubmissionAsync(submission);
                    TempData["Success"] = "Nộp bài tập thành công!";
                    _logger.LogInformation("User {UserId} submitted assignment {AssignmentId}",
                        userId, assignmentId);
                }

                return RedirectToAction(nameof(MySubmissions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assignment {AssignmentId}", assignmentId);
                TempData["Error"] = "Có lỗi xảy ra khi nộp bài tập.";
                return RedirectToAction(nameof(SubmitAssignment), new { id = assignmentId });
            }
        }

        // ============================================
        // 10. MY SUBMISSIONS
        // ============================================
        [HttpGet("MySubmissions")]
        public async Task<IActionResult> MySubmissions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);

                var totalSubmissions = submissions.Count;
                var gradedSubmissions = submissions.Count(s => s.Grade.HasValue);
                var pendingSubmissions = submissions.Count(s => !s.Grade.HasValue);
                var avgGrade = await _submissionRepo.GetAverageGradeAsync(userId);

                ViewBag.TotalSubmissions = totalSubmissions;
                ViewBag.GradedCount = gradedSubmissions;
                ViewBag.PendingCount = pendingSubmissions;
                ViewBag.AverageGrade = Math.Round(avgGrade, 1);

                return View(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student submissions");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách bài nộp.";
                return View(new List<Submission>());
            }
        }

        // ============================================
        // 11. SUBMISSION DETAILS
        // ============================================
        [HttpGet("SubmissionDetails/{id}")]
        public async Task<IActionResult> SubmissionDetails(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var submission = await _submissionRepo.GetSubmissionByIdAsync(id);

                if (submission == null)
                {
                    TempData["Error"] = "Không tìm thấy bài nộp!";
                    return RedirectToAction(nameof(MySubmissions));
                }

                if (submission.StudentId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access submission {SubmissionId}",
                        userId, id);
                    TempData["Error"] = "Bạn không có quyền xem bài nộp này!";
                    return RedirectToAction(nameof(MySubmissions));
                }

                return View(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading submission details {SubmissionId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin bài nộp.";
                return RedirectToAction(nameof(MySubmissions));
            }
        }

        // ============================================
        // 12. DOWNLOAD FILE
        // ============================================
        [HttpGet("DownloadFile/{submissionId}")]
        public async Task<IActionResult> DownloadFile(Guid submissionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var submission = await _submissionRepo.GetSubmissionByIdAsync(submissionId);

                if (submission == null)
                {
                    return NotFound("Không tìm thấy bài nộp!");
                }

                // Check permission
                if (submission.StudentId != userId)
                {
                    _logger.LogWarning("Unauthorized download attempt for submission {SubmissionId}",
                        submissionId);
                    return Forbid();
                }

                if (string.IsNullOrEmpty(submission.FileUrl))
                {
                    return NotFound("File không tồn tại!");
                }

                // Get physical path
                var filePath = _fileUploadService.GetPhysicalPath(submission.FileUrl);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("File not found at path: {FilePath}", filePath);
                    return NotFound("File không tồn tại trên server!");
                }

                // Return file
                var fileName = _fileUploadService.GetFileName(submission.FileUrl);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = "application/octet-stream";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file for submission {SubmissionId}",
                    submissionId);
                TempData["Error"] = "Có lỗi xảy ra khi tải file.";
                return RedirectToAction(nameof(MySubmissions));
            }
        }

        // ============================================
        // 13. GRADES
        // ============================================
        [HttpGet("Grades")]
        public async Task<IActionResult> Grades()
        {
            try
            {
                var userId = GetCurrentUserId();
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);
                var enrollments = await _enrollmentRepo.GetEnrollmentsByUserIdAsync(userId);

                var gradedSubmissions = submissions.Where(s => s.Grade.HasValue).ToList();
                var avgGrade = gradedSubmissions.Any()
                    ? gradedSubmissions.Average(s => s.Grade!.Value)
                    : 0;
                var maxGrade = gradedSubmissions.Any()
                    ? gradedSubmissions.Max(s => s.Grade!.Value)
                    : 0;
                var minGrade = gradedSubmissions.Any()
                    ? gradedSubmissions.Min(s => s.Grade!.Value)
                    : 0;

                ViewBag.AverageGrade = Math.Round(avgGrade, 1);
                ViewBag.GradedCount = gradedSubmissions.Count;
                ViewBag.MaxGrade = Math.Round(maxGrade, 1);
                ViewBag.MinGrade = Math.Round(minGrade, 1);
                ViewBag.Enrollments = enrollments;

                return View(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student grades");
                TempData["Error"] = "Có lỗi xảy ra khi tải bảng điểm.";
                return View(new List<Submission>());
            }
        }

        // ============================================
        // 14. REPORTS
        // ============================================
        [HttpGet("Reports")]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var userId = GetCurrentUserId();

                var enrollments = await _enrollmentRepo.GetEnrollmentsByUserIdAsync(userId);
                var assignments = await _assignmentRepo.GetAssignmentsByStudentIdAsync(userId);
                var submissions = await _submissionRepo.GetSubmissionsByStudentIdAsync(userId);
                var upcomingAssignments = await _assignmentRepo.GetUpcomingAssignmentsAsync(userId, 3);
                var overdueAssignments = await _assignmentRepo.GetOverdueAssignmentsAsync(userId);

                var activeCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Enrolled);
                var completedAssignments = submissions.Count;
                var totalHours = assignments.Count * 2; // Estimate
                var avgGrade = await _submissionRepo.GetAverageGradeAsync(userId);

                // ✅ Calculate on-time submission rate
                var onTimeSubmissions = submissions.Count(s =>
                    s.Assignment != null &&
                    s.SubmittedAt <= s.Assignment.DueDate);
                var onTimeRate = submissions.Any()
                    ? (double)onTimeSubmissions / submissions.Count * 100
                    : 0;

                ViewBag.ActiveCourses = activeCourses;
                ViewBag.CompletedAssignments = completedAssignments;
                ViewBag.TotalHours = totalHours;
                ViewBag.AverageGrade = Math.Round(avgGrade, 1);
                ViewBag.OnTimeRate = Math.Round(onTimeRate, 0);
                ViewBag.CompletionRate = assignments.Any()
                    ? Math.Round((double)completedAssignments / assignments.Count * 100, 0)
                    : 0;
                ViewBag.UpcomingAssignments = upcomingAssignments;
                ViewBag.OverdueAssignments = overdueAssignments;
                ViewBag.Enrollments = enrollments;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student reports");
                TempData["Error"] = "Có lỗi xảy ra khi tải báo cáo.";
                return View();
            }
        }
    }
}