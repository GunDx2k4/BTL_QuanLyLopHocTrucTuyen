using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;


namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class CourseInstructorController : BaseInstructorController
    {
        private readonly ApplicationDbContext _context;

        public CourseInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Course()
        {
            // ✅ Lấy ID giảng viên từ Claim
            var instructorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(instructorIdClaim))
            {
                // Nếu không có claim (chưa đăng nhập), chuyển về login
                return Redirect("/Home/Login");
            }

            var instructorId = Guid.Parse(instructorIdClaim);

            // ✅ Truy vấn các khóa học của giảng viên này
            var courses = await _context.Courses
                .Include(c => c.Tenant)
                .Include(c => c.Enrollments)
                .Include(c => c.Instructor)
                .Where(c => c.InstructorId == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View("~/Views/Instructor/CourseInstructor/Course.cshtml", courses);
        }
        [HttpPost]
        public async Task<IActionResult> SelectCourse(Guid courseId)
        {
            if (courseId == Guid.Empty)
                return BadRequest(new { message = "CourseId không hợp lệ!" });

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound(new { message = "Không tìm thấy khóa học!" });

            // ✅ Lấy danh tính hiện tại
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
                return Unauthorized(new { message = "Bạn chưa đăng nhập!" });

            // ✅ Xóa claim cũ (nếu có)
            var oldClaim = identity.FindFirst("CurrentCourseId");
            if (oldClaim != null)
                identity.RemoveClaim(oldClaim);

            // ✅ Thêm claim mới (CourseId)
            identity.AddClaim(new Claim("CurrentCourseId", courseId.ToString()));

            // ✅ (Tùy chọn) thêm tên khóa học
            var oldNameClaim = identity.FindFirst("CurrentCourseName");
            if (oldNameClaim != null)
                identity.RemoveClaim(oldNameClaim);
            identity.AddClaim(new Claim("CurrentCourseName", course.Name));

            // ✅ Cập nhật lại cookie xác thực
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );


            // ✅ Lưu danh sách khóa học gần đây vào cookie (JSON)
            var recentCookie = Request.Cookies["RecentCourses"];
            List<Guid> recentIds = new();

            if (!string.IsNullOrEmpty(recentCookie))
            {
                recentIds = recentCookie.Split(',')
                    .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .ToList();
            }

            // Thêm courseId mới lên đầu danh sách
            recentIds.Remove(courseId); // tránh trùng
            recentIds.Insert(0, courseId);

            // Giữ tối đa 5 khóa học gần đây
            if (recentIds.Count > 5)
                recentIds = recentIds.Take(5).ToList();

            // Ghi cookie lại
            Response.Cookies.Append("RecentCourses", string.Join(",", recentIds), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7)
            });

            return Ok(new { message = $"Đã chọn khóa học: {course.Name}" });
        }

    }
}
