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

            var effectiveCourseId = courseId ?? GetCurrentCourseId();

            ViewBag.Courses = courses;
            ViewBag.CurrentCourseId = effectiveCourseId;

            // 🔹 Lấy danh sách bài tập của khóa học hiện tại
            var assignments = await _context.Assignments
                .Include(a => a.Lesson)
                .Where(a => effectiveCourseId.HasValue && a.Lesson.CourseId == effectiveCourseId.Value)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Assignments = assignments;
            ViewBag.CurrentAssignmentId = assignmentId ?? Guid.Empty;

            // 🔹 Lấy danh sách bài nộp (Submissions)
            var submissionsQuery = _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                .AsQueryable();

            // ✅ Chỉ hiển thị bài nộp trong khóa học hiện tại
            if (effectiveCourseId.HasValue)
                submissionsQuery = submissionsQuery.Where(s => s.Assignment.Lesson.CourseId == effectiveCourseId.Value);

            // ✅ Nếu chọn bài tập cụ thể → lọc thêm theo assignmentId
            if (assignmentId.HasValue && assignmentId.Value != Guid.Empty)
                submissionsQuery = submissionsQuery.Where(s => s.AssignmentId == assignmentId.Value);

            var submissions = await submissionsQuery
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            // 🔹 Trả dữ liệu về View
            return View("~/Views/Instructor/GradeInstructor/Grade.cshtml", submissions);
        }

        [HttpGet]
        public async Task<IActionResult> DetailGrade(Guid id)
        {
            // 🔹 Lấy submission theo ID
            var submission = await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                return NotFound();

            // 🔹 Gửi dữ liệu sang View
            return View("~/Views/Instructor/GradeInstructor/DetailGrade.cshtml", submission);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGrade(Guid id, float grade)
        {
            var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id);
            if (submission == null)
                return NotFound();

            submission.Grade = grade;

            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Đã chấm điểm thành công!";
            return RedirectToAction("DetailGrade", new { id });
        }

    }
}
