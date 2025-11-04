using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class GradeInstructorController : BaseInstructorController
    {
        private readonly ApplicationDbContext _context;

        public GradeInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Grade(Guid? courseId, Guid? assignmentId)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null)
                return Redirect("/Home/Login");

            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            // 🔹 Lấy danh sách khóa học mà giảng viên phụ trách
            var courses = await _context.Courses
                .Where(c => c.InstructorId.ToString() == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.CurrentCourseId = courseId ?? GetCurrentCourseId();

            // 🔹 Lấy danh sách bài tập thuộc khóa học hiện tại
            var assignments = await _context.Assignments
                .Include(a => a.Lesson)
                .Where(a => a.Lesson.CourseId == courseId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Assignments = assignments;

            // ✅ Nếu chưa chọn assignment thì mặc định chọn bài đầu tiên
            if (!assignmentId.HasValue && assignments.Any())
                assignmentId = assignments.First().Id;

            ViewBag.CurrentAssignmentId = assignmentId ?? Guid.Empty;

            // 🔹 Lấy danh sách bài nộp (Submissions)
            var submissionsQuery = _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                .AsQueryable();

            if (courseId.HasValue)
                submissionsQuery = submissionsQuery.Where(s => s.Assignment.Lesson.CourseId == courseId);

            if (assignmentId.HasValue)
                submissionsQuery = submissionsQuery.Where(s => s.AssignmentId == assignmentId);

            var submissions = await submissionsQuery
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            // 🔹 Gửi dữ liệu sang View
            return View("~/Views/Instructor/GradeInstructor/Grade.cshtml", submissions);
        }

        [HttpGet]
        public IActionResult DetailGrade(Guid id)
        {
            return View("~/Views/Instructor/GradeInstructor/DetailGrade.cshtml");
        }
    }
}
