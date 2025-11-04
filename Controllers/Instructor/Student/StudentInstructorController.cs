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
            // ✅ Nếu không truyền courseId thì lấy từ Claim
            if (!courseId.HasValue)
            {
                var courseIdClaim = User.FindFirst("CurrentCourseId")?.Value;
                if (!string.IsNullOrEmpty(courseIdClaim))
                    courseId = Guid.Parse(courseIdClaim);
            }

            var courses = await _context.Courses
                .Where(c => c.InstructorId.ToString() == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.CurrentCourseId = courseId;
            ViewBag.CurrentCourseName = courses.FirstOrDefault(c => c.Id == courseId)?.Name ?? "Tất cả khóa học";
            ViewBag.SelectedStatus = status;

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
            var list = await enrollments.ToListAsync();
            ViewBag.TotalStudents = list.Count;
            ViewBag.ActiveCount = list.Count(e => e.Status == EnrollmentStatus.Enrolled);
            ViewBag.QuitCount = list.Count(e => e.Status == EnrollmentStatus.Dropped);


            return View("~/Views/Instructor/StudentInstructor/Student.cshtml",list);
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
