using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class AssignmentInstructorController : Controller
    {
        private readonly SqlServerDbContext _context;

        public AssignmentInstructorController(SqlServerDbContext context)
        {
            _context = context;
        }

        /* =====================================================
           📋 DANH SÁCH BÀI TẬP
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> Assignment()
        {
            var assignments = await _context.Assignments
                .Include(a => a.Lesson)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View("~/Views/Instructor/AssignmentInstructor/Assignment.cshtml", assignments);
        }

        /* =====================================================
           ➕ THÊM BÀI TẬP (CHO PHÉP CẢ LINK NGOÀI + FILE)
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> AddAssignment(Guid? lessonId)
        {
            ViewBag.Lessons = await _context.Lessons
                .OrderBy(l => l.Title)
                .ToListAsync();

            var assignment = new Assignment
            {
                LessonId = lessonId ?? Guid.Empty
            };

            return View("~/Views/Instructor/AssignmentInstructor/AddAssignment.cshtml", assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignment([FromForm] Assignment assignment)
        {
            Console.WriteLine("===== 🧩 BẮT ĐẦU XỬ LÝ THÊM BÀI TẬP =====");
            Console.WriteLine($"📘 Tiêu đề: {assignment.Title}");
            Console.WriteLine($"🌐 Link ngoài: {assignment.ExternalFileUrl ?? "(trống)"}");
            Console.WriteLine($"📂 File upload: {(assignment.UploadFile != null ? assignment.UploadFile.FileName : "Không có")}");

            if (!ModelState.IsValid)
            {
                ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
                return View("~/Views/Instructor/AssignmentInstructor/AddAssignment.cshtml", assignment);
            }

            assignment.Id = Guid.NewGuid();
            assignment.CreatedAt = DateTime.Now;

            // 🟢 Xử lý file upload từ máy (nếu có)
            if (assignment.UploadFile != null && assignment.UploadFile.Length > 0)
            {
                try
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/assignments");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(assignment.UploadFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await assignment.UploadFile.CopyToAsync(stream);
                    }

                    assignment.UploadedFileUrl = "/uploads/assignments/" + fileName;
                    assignment.UploadedFileName = assignment.UploadFile.FileName;

                    Console.WriteLine($"✅ Đã lưu file nội bộ: {assignment.UploadedFileUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi khi lưu file: {ex.Message}");
                }
            }

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            Console.WriteLine("🎉 Đã lưu bài tập thành công!");
            TempData["SuccessMessage"] = "✅ Thêm bài tập thành công!";
            return RedirectToAction(nameof(Assignment));
        }

        /* =====================================================
           ✏️ CHỈNH SỬA BÀI TẬP
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> EditAssignment(Guid id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();

            return View("~/Views/Instructor/AssignmentInstructor/EditAssignment.cshtml", assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment([FromForm] Assignment assignment)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
                return View("~/Views/Instructor/AssignmentInstructor/EditAssignment.cshtml", assignment);
            }

            var existing = await _context.Assignments.FindAsync(assignment.Id);
            if (existing == null)
                return NotFound();

            // ✅ Cập nhật các trường cơ bản
            existing.Title = assignment.Title;
            existing.Description = assignment.Description;
            existing.MaxScore = assignment.MaxScore;
            existing.Type = assignment.Type;
            existing.AvailableFrom = assignment.AvailableFrom;
            existing.AvailableUntil = assignment.AvailableUntil;
            existing.LessonId = assignment.LessonId;
            existing.ExternalFileUrl = assignment.ExternalFileUrl;

            // ✅ Nếu người dùng upload file mới → ghi đè file cũ
            if (assignment.UploadFile != null && assignment.UploadFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/assignments");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(assignment.UploadFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await assignment.UploadFile.CopyToAsync(stream);
                }

                existing.UploadedFileUrl = "/uploads/assignments/" + fileName;
                existing.UploadedFileName = assignment.UploadFile.FileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "✅ Cập nhật bài tập thành công!";
            return RedirectToAction(nameof(Assignment));
        }

        /* =====================================================
           🗑️ XÓA BÀI TẬP
        ===================================================== */
        [HttpDelete]
        public async Task<IActionResult> DeleteAssignment(Guid id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
                return Json(new { success = false, message = "Không tìm thấy bài tập để xóa!" });

            try
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /* =====================================================
           🌍 CÔNG KHAI / ẨN BÀI TẬP
        ===================================================== */
        [HttpPost]
        public async Task<IActionResult> TogglePublicAssignment(Guid id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
                return Json(new { success = false, message = "Không tìm thấy bài tập." });

            assignment.IsPublic = !assignment.IsPublic;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isPublic = assignment.IsPublic });
        }
    }
}
