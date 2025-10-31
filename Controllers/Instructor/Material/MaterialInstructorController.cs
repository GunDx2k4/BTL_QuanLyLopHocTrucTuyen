using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Services;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class MaterialInstructorController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly SupabaseStorageService _supabaseStorage;

        public MaterialInstructorController(SqlServerDbContext context, SupabaseStorageService supabaseStorage)
        {
            _context = context;
            _supabaseStorage = supabaseStorage;
        }

        /* =====================================================
           📚 DANH SÁCH TÀI LIỆU
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> Material()
        {
            var materials = await _context.Materials
                .Include(m => m.Lesson)
                .Include(m => m.Uploader)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            return View("~/Views/Instructor/MaterialInstructor/Material.cshtml", materials);
        }

        /* =====================================================
           ➕ THÊM TÀI LIỆU
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> AddMaterial(Guid? lessonId)
        {
            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
            var material = new Material { LessonId = lessonId ?? Guid.Empty };
            return View("~/Views/Instructor/MaterialInstructor/AddMaterial.cshtml", material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial([FromForm] Material material)
        {
            Console.WriteLine("===== 🧩 BẮT ĐẦU XỬ LÝ THÊM TÀI LIỆU =====");
            Console.WriteLine($"📘 Tiêu đề: {material.Title}");

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList() });
            }

            material.Id = Guid.NewGuid();
            material.UploadedAt = DateTime.Now;

            try
            {
                // ✅ Upload file lên Supabase (nếu có)
                if (material.UploadFile != null && material.UploadFile.Length > 0)
                {
                    var publicUrl = await _supabaseStorage.UploadFileAsync(material.UploadFile, "materials");
                    material.UploadedFileUrl = publicUrl;
                    material.UploadedFileName = material.UploadFile.FileName;
                }

                _context.Materials.Add(material);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Đã thêm tài liệu '{material.Title}' với file: {material.UploadedFileUrl}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi thêm tài liệu: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }


        /* =====================================================
           ✏️ CHỈNH SỬA TÀI LIỆU
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> EditMaterial(Guid id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
            return View("~/Views/Instructor/MaterialInstructor/EditMaterial.cshtml", material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial([FromForm] Material material)
        {
            var existing = await _context.Materials.FindAsync(material.Id);
            if (existing == null)
                return NotFound();

            try
            {
                existing.Title = material.Title;
                existing.Description = material.Description;
                existing.LessonId = material.LessonId;
                existing.IsPublic = material.IsPublic;
                existing.ExternalFileUrl = material.ExternalFileUrl;

                // ✅ Nếu có file mới thì xóa file cũ rồi upload lại
                if (material.UploadFile != null && material.UploadFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existing.UploadedFileUrl))
                        await _supabaseStorage.DeleteFileAsync(existing.UploadedFileUrl);

                    var newUrl = await _supabaseStorage.UploadFileAsync(material.UploadFile, "materials");
                    existing.UploadedFileUrl = newUrl;
                    existing.UploadedFileName = material.UploadFile.FileName;
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"✏️ Đã cập nhật tài liệu '{existing.Title}'");
                TempData["SuccessMessage"] = "✅ Cập nhật tài liệu thành công!";
                return RedirectToAction(nameof(Material));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi EditMaterial: {ex.Message}");
                return View("~/Views/Instructor/MaterialInstructor/EditMaterial.cshtml", material);
            }
        }

        /* =====================================================
           🗑️ XÓA TÀI LIỆU
        ===================================================== */
        [HttpDelete]
        public async Task<IActionResult> DeleteMaterial(Guid id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return Json(new { success = false, message = "Không tìm thấy tài liệu!" });

            try
            {
                if (!string.IsNullOrEmpty(material.UploadedFileUrl))
                    await _supabaseStorage.DeleteFileAsync(material.UploadedFileUrl);

                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();

                Console.WriteLine($"🗑️ Đã xóa tài liệu '{material.Title}'");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi DeleteMaterial: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /* =====================================================
           🌍 CÔNG KHAI / ẨN TÀI LIỆU
        ===================================================== */
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> TogglePublicMaterial(Guid id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return Json(new { success = false, message = "Không tìm thấy tài liệu." });

            try
            {
                material.IsPublic = !material.IsPublic;
                await _context.SaveChangesAsync();

                Console.WriteLine($"🌍 Đã cập nhật công khai: {material.Title} = {material.IsPublic}");
                return Json(new { success = true, isPublic = material.IsPublic });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
