using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Data;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class AccoundInstructorController : BaseInstructorController
    {
        private readonly ApplicationDbContext _context;

        public AccoundInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🧑‍🏫 Trang cá nhân
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null)
                return Redirect("/Home/Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == instructorId);
            if (user == null)
                return NotFound("Không tìm thấy thông tin giảng viên.");

            return View("~/Views/Instructor/AccoundInstructor/Profile.cshtml", user);
        }

        // ⚙️ Trang cài đặt
        [HttpGet]
        public async Task<IActionResult> Setting()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null)
                return Redirect("/Home/Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == instructorId);
            if (user == null)
                return NotFound("Không tìm thấy tài khoản.");

            return View("~/Views/Instructor/AccoundInstructor/Setting.cshtml", user);
        }

        // ⚙️ POST: cập nhật tên hiển thị
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setting(Guid id, string fullName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(fullName))
                user.FullName = fullName.Trim();

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(); // ✅ quan trọng: trả về 200 OK cho AJAX
        }
    }
}
