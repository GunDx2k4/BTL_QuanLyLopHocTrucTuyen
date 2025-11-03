using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class StatisticInstructorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Statistic()
        {
            // ✅ Lấy ID giảng viên đăng nhập
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null)
                return Redirect("/Home/Login");

            // ✅ Lấy khóa học hiện tại trong Claim (nếu có)
            var courseIdClaim = User.FindFirst("CurrentCourseId")?.Value;

            // ✅ Lấy toàn bộ các khóa học của giảng viên này
            var courses = await _context.Courses
                .Include(c => c.Tenant)
                .Where(c => c.InstructorId.ToString() == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;

            // ✅ Nếu có courseId trong claim → highlight
            if (!string.IsNullOrEmpty(courseIdClaim) && Guid.TryParse(courseIdClaim, out var currentId))
            {
                ViewBag.CurrentCourseId = currentId;
                var currentCourse = courses.FirstOrDefault(c => c.Id == currentId);
                ViewBag.CurrentCourseName = currentCourse?.Name ?? "Chưa chọn khóa học";
            }
            else
            {
                ViewBag.CurrentCourseId = Guid.Empty;
                ViewBag.CurrentCourseName = "Chưa chọn khóa học";
            }

            // Hiện tại demo tĩnh, sau này có thể load thống kê thực tế
            return View("~/Views/Instructor/StatisticInstructor/Statistic.cshtml");
        }
    }
}
