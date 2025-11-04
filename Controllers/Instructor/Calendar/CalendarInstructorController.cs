using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Data;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using System.Security.Claims;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class CalendarInstructorController : BaseInstructorController
    {
        
        private readonly SqlServerDbContext _context;

        public CalendarInstructorController(SqlServerDbContext context)
        {
            _context = context;
        }

        /* ==========================
           📅 TRANG LỊCH CHÍNH
        ========================== */
        [HttpGet]
        public async Task<IActionResult> Calendar()
        {
            // 🔹 Lấy ID giảng viên đang đăng nhập
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null)
                return Redirect("/Home/Login");

            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            // 🔹 Lấy danh sách khóa học của giảng viên này
            var courses = await _context.Courses
                .Where(c => c.InstructorId.ToString() == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses; // Để hiển thị dropdown chọn khóa học

            var currentCourseId = User.FindFirst("CurrentCourseId")?.Value;
            ViewBag.CurrentCourseId = currentCourseId ?? "all";

            return View("~/Views/Instructor/CalendarInstructor/Calendar.cshtml");
        }

        /* ==========================
   📤 API JSON CHO FULLCALENDAR
    Gộp cả Bài học (Lesson) và Bài tập (Assignment)
    ========================== */
    [HttpGet]
    public async Task<IActionResult> GetEvents(Guid? courseId)
    {
        // 🔹 Lấy ID giảng viên đang đăng nhập
        var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (instructorId == null)
            return Unauthorized();

        // 🔹 Lấy toàn bộ khóa học mà giảng viên này dạy
        var courseIds = await _context.Courses
            .Where(c => c.InstructorId.ToString() == instructorId)
            .Select(c => c.Id)
            .ToListAsync();

        // ======================
        // 🔸 1️⃣ Bài học (Lesson)
        // ======================
        var lessonsQuery = _context.Lessons
            .Include(l => l.Course)
            .Where(l => l.Course != null && courseIds.Contains(l.Course.Id))
            .AsQueryable();

        if (courseId.HasValue && courseId != Guid.Empty)
        {
            lessonsQuery = lessonsQuery.Where(l => l.CourseId == courseId);
        }

        var lessons = await lessonsQuery
            .Select(l => new
            {
                id = l.Id,
                title = l.Title,
                type = "Bài học",
                start = (DateTime?)l.BeginTime,
                end = (DateTime?)l.EndTime,
                description = l.Course != null ? $"Khóa: {l.Course.Name}" : "",
                courseId = l.CourseId
            })
            .ToListAsync();

        // ======================
        // 🔸 2️⃣ Bài tập (Assignment)
        // ======================
        var assignmentsQuery = _context.Assignments
            .Include(a => a.Lesson)
            .ThenInclude(l => l.Course)
            .AsQueryable();

        if (courseId.HasValue && courseId != Guid.Empty)
        {
            var cid = courseId.Value;
            assignmentsQuery = assignmentsQuery
                .Where(a => a.Lesson != null && a.Lesson.CourseId == cid);
        }
        else
        {
            assignmentsQuery = assignmentsQuery
                .Where(a => a.Lesson != null
                    && a.Lesson.CourseId.HasValue
                    && courseIds.Contains(a.Lesson.CourseId.Value));
        }

        var assignments = await assignmentsQuery
            .Select(a => new
            {
                id = a.Id,
                title = a.Title,
                type = a.Type,
                start = a.AvailableFrom,
                end = a.AvailableUntil,
                description = a.Description,
                courseId = a.Lesson.CourseId
            })
            .ToListAsync();

        // 🔹 Gộp cả hai loại sự kiện
        var allEvents = lessons.Concat(assignments);

        return Json(allEvents);
    }

        /* ==========================
           ✏️ NÚT SỬA (REDIRECT)
        ========================== */
        [HttpGet]
        [Route("/Instructor/EditAssignmentRedirect/{id:guid}")]
        public IActionResult EditAssignment(Guid id)
        {
            // Điều hướng sang trang EditAssignment thật
            return Redirect($"/Instructor/EditAssignment?id={id}");
        }
        /* ==========================
        ✏️ NÚT SỬA BÀI HỌC (REDIRECT)
        ========================== */
        [HttpGet]
        [Route("/Instructor/EditLessonRedirect/{id:guid}")]
        public IActionResult EditLesson(Guid id)
        {
            // Điều hướng sang trang chỉnh sửa bài học thật
            return Redirect($"/Instructor/EditLesson?id={id}");
        }
    }
}
