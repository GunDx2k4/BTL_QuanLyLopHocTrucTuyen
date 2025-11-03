using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Instructor
{
    [Route("Instructor")]
    public class HomeInstructorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instructorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorIdClaim))
                return Redirect("/Home/Login");

            var instructorId = Guid.Parse(instructorIdClaim);

            // ✅ Lấy danh sách khóa học gần đây từ cookie
            var cookie = Request.Cookies["RecentCourses"];
            List<Guid> recentIds = new();
            if (!string.IsNullOrEmpty(cookie))
            {
                recentIds = cookie.Split(',')
                    .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .ToList();
            }

            // ✅ Truy vấn khóa học thật theo ID
            var recentCourses = await _context.Courses
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == instructorId && recentIds.Contains(c.Id))
                .ToListAsync();

            // ✅ Sắp xếp đúng thứ tự cookie (gần nhất trước)
            var ordered = recentCourses.OrderBy(c => recentIds.IndexOf(c.Id)).ToList();

            ViewBag.RecentCourses = ordered;
            return View("~/Views/Instructor/Index.cshtml");
        }
    }
}
