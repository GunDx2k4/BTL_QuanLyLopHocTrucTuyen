using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Services;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using System.Security.Claims;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class MaterialInstructorController : BaseInstructorController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMaterialRepository _materialRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly SupabaseStorageService _supabaseStorage;

        public MaterialInstructorController(
            ApplicationDbContext context,
            IMaterialRepository materialRepository,
            ILessonRepository lessonRepository,
            SupabaseStorageService supabaseStorage)
        {
            _context = context;
            _materialRepository = materialRepository;
            _lessonRepository = lessonRepository;
            _supabaseStorage = supabaseStorage;
        }

        /* =====================================================
           📚 DANH SÁCH TÀI LIỆU
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> Material()
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            var courseId = GetCurrentCourseId()!.Value;

            var materials = await _context.Materials
                .Include(m => m.Lesson)
                .Include(m => m.Uploader) 
                .Where(m => m.Lesson != null && m.Lesson.CourseId == courseId)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            ViewBag.CourseName = GetCurrentCourseName();

            return View("~/Views/Instructor/MaterialInstructor/Material.cshtml", materials);
        }

        /* =====================================================
           ➕ THÊM TÀI LIỆU
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> AddMaterial(Guid? lessonId)
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            var courseId = GetCurrentCourseId()!.Value;

            ViewBag.Lessons = (await _lessonRepository.FindAsync())
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Title)
                .ToList();

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
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            material.Id = Guid.NewGuid();
            material.UploadedAt = DateTime.Now;

            // ✅ Gán người tải lên
            var instructorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(instructorIdClaim))
                material.UploadedBy = Guid.Parse(instructorIdClaim);

            try
            {
                if (material.UploadFile != null && material.UploadFile.Length > 0)
                {
                    var publicUrl = await _supabaseStorage.UploadFileAsync(material.UploadFile, "materials");
                    material.UploadedFileUrl = publicUrl;
                    material.UploadedFileName = material.UploadFile.FileName;
                }

                await _materialRepository.AddAsync(material);

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
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            var courseId = GetCurrentCourseId()!.Value;

            var material = await _context.Materials
                .Include(m => m.Lesson)
                .FirstOrDefaultAsync(m => m.Id == id && m.Lesson!.CourseId == courseId);

            if (material == null)
                return NotFound();

            ViewBag.Lessons = (await _lessonRepository.FindAsync())
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Title)
                .ToList();

            return View("~/Views/Instructor/MaterialInstructor/EditMaterial.cshtml", material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial([FromForm] Material material)
        {
            var existing = await _materialRepository.FindByIdAsync(material.Id);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy tài liệu!" });

            try
            {
                existing.Title = material.Title;
                existing.Description = material.Description;
                existing.LessonId = material.LessonId;
                existing.IsPublic = material.IsPublic;
                existing.ExternalFileUrl = material.ExternalFileUrl;

                if (material.UploadFile != null && material.UploadFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existing.UploadedFileUrl))
                        await _supabaseStorage.DeleteFileAsync(existing.UploadedFileUrl);

                    var newUrl = await _supabaseStorage.UploadFileAsync(material.UploadFile, "materials");
                    existing.UploadedFileUrl = newUrl;
                    existing.UploadedFileName = material.UploadFile.FileName;
                }

                await _materialRepository.UpdateAsync(existing);
                Console.WriteLine($"✏️ Cập nhật tài liệu: {existing.Title}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi EditMaterial: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /* =====================================================
           🗑️ XÓA TÀI LIỆU
        ===================================================== */
        [HttpDelete]
        public async Task<IActionResult> DeleteMaterial(Guid id)
        {
            var material = await _materialRepository.FindByIdAsync(id);
            if (material == null)
                return Json(new { success = false, message = "Không tìm thấy tài liệu!" });

            try
            {
                if (!string.IsNullOrEmpty(material.UploadedFileUrl))
                    await _supabaseStorage.DeleteFileAsync(material.UploadedFileUrl);

                await _materialRepository.DeleteByIdAsync(id);
                Console.WriteLine($"🗑️ Xóa tài liệu: {material.Title}");
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
            var material = await _materialRepository.FindByIdAsync(id);
            if (material == null)
                return Json(new { success = false, message = "Không tìm thấy tài liệu." });

            try
            {
                material.IsPublic = !material.IsPublic;
                await _materialRepository.UpdateAsync(material);

                Console.WriteLine($"🌍 Đổi trạng thái công khai: {material.Title} = {material.IsPublic}");
                return Json(new { success = true, isPublic = material.IsPublic });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
