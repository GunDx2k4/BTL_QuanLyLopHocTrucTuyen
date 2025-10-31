using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class AssignmentInstructorController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly SupabaseStorageService _supabaseStorage;

        public AssignmentInstructorController(SqlServerDbContext context, SupabaseStorageService supabaseStorage)
        {
            _context = context;
            _supabaseStorage = supabaseStorage;
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
           ➕ THÊM BÀI TẬP (Supabase)
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> AddAssignment(Guid? lessonId)
        {
            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
            var assignment = new Assignment { LessonId = lessonId ?? Guid.Empty };
            return View("~/Views/Instructor/AssignmentInstructor/AddAssignment.cshtml", assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignment([FromForm] Assignment assignment)
        {
            Console.WriteLine("===== 🧩 BẮT ĐẦU XỬ LÝ THÊM BÀI TẬP =====");
            Console.WriteLine($"📘 Tiêu đề: {assignment.Title}");

            if (!ModelState.IsValid)
            {
                ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
                return View("~/Views/Instructor/AssignmentInstructor/AddAssignment.cshtml", assignment);
            }

            assignment.Id = Guid.NewGuid();
            assignment.CreatedAt = DateTime.Now;

            try
            {
                // ✅ Nếu có file upload → lưu lên Supabase
                if (assignment.UploadFile != null && assignment.UploadFile.Length > 0)
                {
                    var url = await _supabaseStorage.UploadFileAsync(assignment.UploadFile, "assignments");
                    assignment.UploadedFileUrl = url;
                    assignment.UploadedFileName = assignment.UploadFile.FileName;
                }

                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Đã thêm bài tập '{assignment.Title}' với file: {assignment.UploadedFileUrl}");
                TempData["SuccessMessage"] = "✅ Thêm bài tập thành công!";
                return RedirectToAction(nameof(Assignment));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi thêm bài tập: {ex.Message}");
                ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync(); // ✅ Thêm dòng này
                return View("~/Views/Instructor/AssignmentInstructor/AddAssignment.cshtml", assignment);
            }

        }

        /* =====================================================
           ✏️ CHỈNH SỬA BÀI TẬP
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> EditAssignment(Guid id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();

            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
            return View("~/Views/Instructor/AssignmentInstructor/EditAssignment.cshtml", assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment([FromForm] Assignment assignment)
        {
            var existing = await _context.Assignments.FindAsync(assignment.Id);
            if (existing == null)
                return NotFound();

            try
            {
                existing.Title = assignment.Title;
                existing.Description = assignment.Description;
                existing.MaxScore = assignment.MaxScore;
                existing.Type = assignment.Type;
                existing.AvailableFrom = assignment.AvailableFrom;
                existing.AvailableUntil = assignment.AvailableUntil;
                existing.LessonId = assignment.LessonId;
                existing.ExternalFileUrl = assignment.ExternalFileUrl;

                // ✅ Nếu có file mới → xóa file cũ rồi upload lại
                if (assignment.UploadFile != null && assignment.UploadFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existing.UploadedFileUrl))
                        await _supabaseStorage.DeleteFileAsync(existing.UploadedFileUrl);

                    var newUrl = await _supabaseStorage.UploadFileAsync(assignment.UploadFile, "assignments");
                    existing.UploadedFileUrl = newUrl;
                    existing.UploadedFileName = assignment.UploadFile.FileName;
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"✏️ Đã cập nhật bài tập '{existing.Title}'");
                TempData["SuccessMessage"] = "✅ Cập nhật bài tập thành công!";
                return RedirectToAction(nameof(Assignment));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi EditAssignment: {ex.Message}");
                return View("~/Views/Instructor/AssignmentInstructor/EditAssignment.cshtml", assignment);
            }
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
                if (!string.IsNullOrEmpty(assignment.UploadedFileUrl))
                    await _supabaseStorage.DeleteFileAsync(assignment.UploadedFileUrl);

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();

                Console.WriteLine($"🗑️ Đã xóa bài tập '{assignment.Title}'");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi DeleteAssignment: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /* =====================================================
           🌍 CÔNG KHAI / ẨN BÀI TẬP
        ===================================================== */
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> TogglePublicAssignment(Guid id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
                return Json(new { success = false, message = "Không tìm thấy bài tập." });

            try
            {
                assignment.IsPublic = !assignment.IsPublic;
                await _context.SaveChangesAsync();

                Console.WriteLine($"🌍 Đã cập nhật công khai: {assignment.Title} = {assignment.IsPublic}");
                return Json(new { success = true, isPublic = assignment.IsPublic });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
