using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class StudentInstructorController : Controller
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

            // ✅ Lấy danh sách khóa học của giảng viên
            var courses = await _context.Courses
                .Where(c => c.InstructorId.ToString() == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;
            ViewBag.SelectedStatus = status;

            // ✅ Lấy danh sách học viên đăng ký khóa học của giảng viên
            var enrollments = _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.Course.InstructorId.ToString() == instructorId)
                .AsQueryable();

            // ✅ Lọc theo Course và Status
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

            // ✅ Tính toán số bài nộp và điểm trung bình
            var students = enrollmentList.Select(e =>
            {
                var studentId = e.UserId;
                var courseIdValue = e.CourseId;

                var submissions = _context.Submissions
                    .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                    .Where(s => s.StudentId == studentId &&
                                s.Assignment.Lesson.CourseId == courseIdValue);

                var submittedCount = submissions.Count();
                var totalAssignments = _context.Assignments
                    .Include(a => a.Lesson)
                    .Count(a => a.Lesson.CourseId == courseIdValue);
                var avgGrade = submissions.Average(s => (double?)s.Grade) ?? 0;

                return new
                {
                    e.User.Id,
                    e.User.FullName,
                    e.User.Email,
                    CourseName = e.Course.Name,
                    Status = e.Status,
                    SubmittedCount = submittedCount,
                    TotalAssignments = totalAssignments,
                    AverageScore = Math.Round(avgGrade, 1)
                };
            }).ToList();

            // ✅ Thống kê nhanh
            ViewBag.TotalStudents = students.Count;
            ViewBag.ActiveCount = students.Count(s => s.Status == EnrollmentStatus.Enrolled);
            ViewBag.QuitCount   = students.Count(s => s.Status == EnrollmentStatus.Dropped);
            ViewBag.Students = students;

            return View("~/Views/Instructor/StudentInstructor/Student.cshtml");
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
