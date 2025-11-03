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
           Lấy bài tập (Assignment) theo khóa học
        ========================== */
        [HttpGet]
        public async Task<IActionResult> GetEvents(Guid? courseId)
        {
            // 🔹 Lấy toàn bộ hoặc lọc theo CourseId
            var query = _context.Assignments
                .Include(a => a.Lesson)
                .AsQueryable();

            if (courseId.HasValue && courseId != Guid.Empty)
            {
                query = query.Where(a => a.Lesson != null && a.Lesson.CourseId == courseId);
            }

            var assignments = await query
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

            return Json(assignments);
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
    }
}
