using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class MaterialInstructorController : Controller
    {
        private readonly SqlServerDbContext _context;

        public MaterialInstructorController(SqlServerDbContext context)
        {
            _context = context;
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
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            material.Id = Guid.NewGuid();
            material.UploadedAt = DateTime.Now;

            // 🔹 Nếu người dùng upload file từ máy
            if (material.UploadFile != null && material.UploadFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/materials");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(material.UploadFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await material.UploadFile.CopyToAsync(stream);
                }

                material.UploadedFileUrl = "/uploads/materials/" + uniqueFileName;
                material.UploadedFileName = material.UploadFile.FileName;
            }

            _context.Add(material);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
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
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var existing = await _context.Materials.FindAsync(material.Id);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy tài liệu!" });

            try
            {
                // ===== Cập nhật các trường cơ bản =====
                existing.Title = material.Title;
                existing.Description = material.Description;
                existing.LessonId = material.LessonId;
                existing.IsPublic = material.IsPublic;
                existing.UploadedAt = material.UploadedAt;
                existing.ExternalFileUrl = material.ExternalFileUrl;

                // ===== Nếu upload file mới =====
                if (material.UploadFile != null && material.UploadFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/materials");
                    Directory.CreateDirectory(uploadsFolder);

                    // Xóa file cũ (nếu có)
                    if (!string.IsNullOrEmpty(existing.UploadedFileUrl))
                    {
                        string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.UploadedFileUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Lưu file mới
                    string uniqueFileName = Guid.NewGuid() + Path.GetExtension(material.UploadFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await material.UploadFile.CopyToAsync(stream);
                    }

                    existing.UploadedFileUrl = "/uploads/materials/" + uniqueFileName;
                    existing.UploadedFileName = material.UploadFile.FileName;
                }

                _context.Materials.Update(existing);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
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
            Console.WriteLine($"🔹 TogglePublic ID = {id}");

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return Json(new { success = false, message = "Không tìm thấy tài liệu." });

            try
            {
                material.IsPublic = !material.IsPublic;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Đã cập nhật IsPublic = {material.IsPublic}");
                return Json(new { success = true, isPublic = material.IsPublic });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi TogglePublic: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
