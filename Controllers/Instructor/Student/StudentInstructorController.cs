using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;


namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class StudentInstructorController : BaseInstructorController
    {
        private readonly ApplicationDbContext _context;

        public StudentInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Student(Guid? courseId, string? status)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null)
                return Redirect("/Home/Login");

            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            // 🔹 Nếu không có courseId → lấy từ Claim
            if (!courseId.HasValue)
            {
                var courseIdClaim = User.FindFirst("CurrentCourseId")?.Value;
                if (!string.IsNullOrEmpty(courseIdClaim))
                    courseId = Guid.Parse(courseIdClaim);
            }

            // 🔹 Lấy danh sách khóa học giảng viên đang dạy
            var courses = await _context.Courses
                .Where(c => c.InstructorId.ToString() == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.CurrentCourseId = courseId;
            ViewBag.CurrentCourseName = courses.FirstOrDefault(c => c.Id == courseId)?.Name ?? "Tất cả khóa học";
            ViewBag.SelectedStatus = status;

            // 🔹 Lấy danh sách enrollment (ghi danh)
            var enrollments = _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.Course.InstructorId.ToString() == instructorId)
                .AsQueryable();

            if (courseId.HasValue && courseId != Guid.Empty)
                enrollments = enrollments.Where(e => e.CourseId == courseId);

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                EnrollmentStatus? enumStatus = status switch
                {
                    "Đang học" => EnrollmentStatus.Enrolled,
                    "Hoàn thành" => EnrollmentStatus.Completed,
                    "Nghỉ học" => EnrollmentStatus.Dropped,
                    _ => null
                };
                if (enumStatus.HasValue)
                    enrollments = enrollments.Where(e => e.Status == enumStatus.Value);
            }

            var enrollmentList = await enrollments.ToListAsync();

            // 🔹 Danh sách kết quả cho view
            var studentData = new List<dynamic>();

            foreach (var e in enrollmentList)
            {
                var studentId = e.UserId;
                var courseIdValue = e.CourseId;

                // ✅ Lấy danh sách bài nộp của học viên này trong khóa học
                var submissions = await _context.Submissions
                    .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                    .Where(s => s.StudentId == studentId && s.Assignment.Lesson.CourseId == courseIdValue)
                    .ToListAsync();

                var submittedCount = submissions.Count;

                // ✅ Tổng số bài tập của khóa học
                var totalAssignments = await _context.Assignments
                    .Include(a => a.Lesson)
                    .CountAsync(a => a.Lesson.CourseId == courseIdValue);

                // ✅ Điểm trung bình: chỉ tính các bài đã chấm (Grade != null)
                double avgGrade = 0;
                if (submissions.Any(s => s.Grade.HasValue))
                {
                    avgGrade = submissions
                        .Where(s => s.Grade.HasValue)
                        .Average(s => (double)s.Grade!.Value);
                }

                studentData.Add(new
                {
                    e.User.Id,
                    e.User.FullName,
                    e.User.Email,
                    CourseName = e.Course?.Name ?? "Không xác định",
                    Status = e.Status,
                    SubmittedCount = submittedCount,
                    TotalAssignments = totalAssignments,
                    AverageScore = Math.Round(avgGrade, 1)
                });
            }

            // 🔹 Thống kê nhanh
            ViewBag.TotalStudents = enrollmentList.Count;
            ViewBag.ActiveCount = enrollmentList.Count(e => e.Status == EnrollmentStatus.Enrolled);
            ViewBag.QuitCount = enrollmentList.Count(e => e.Status == EnrollmentStatus.Dropped);

            // 🔹 Trả về view
            return View("~/Views/Instructor/StudentInstructor/Student.cshtml", studentData);
        }

        [HttpGet]
        public async Task<IActionResult> DetailStudent(Guid id)
        {
            var student = await _context.Users
                .Include(u => u.Submissions)
                .ThenInclude(s => s.Assignment)
                .ThenInclude(a => a.Lesson)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (student == null)
                return NotFound("Không tìm thấy học viên!");

            return View("~/Views/Instructor/StudentInstructor/DetailStudent.cshtml", student);
        }
    }
}
