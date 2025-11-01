using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Services;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class AssignmentInstructorController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly SupabaseStorageService _supabaseStorage;

        public AssignmentInstructorController(
            IAssignmentRepository assignmentRepository,
            ILessonRepository lessonRepository,
            SupabaseStorageService supabaseStorage)
        {
            _assignmentRepository = assignmentRepository;
            _lessonRepository = lessonRepository;
            _supabaseStorage = supabaseStorage;
        }

        /* =====================================================
           📋 DANH SÁCH BÀI TẬP
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> Assignment()
        {
            var assignments = (await _assignmentRepository.FindAsync())
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return View("~/Views/Instructor/AssignmentInstructor/Assignment.cshtml", assignments);
        }

        /* =====================================================
           ➕ THÊM BÀI TẬP (Supabase)
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> AddAssignment(Guid? lessonId)
        {
            ViewBag.Lessons = (await _lessonRepository.FindAsync()).OrderBy(l => l.Title).ToList();
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
                ViewBag.Lessons = (await _lessonRepository.FindAsync()).OrderBy(l => l.Title).ToList();
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

                await _assignmentRepository.AddAsync(assignment);

                Console.WriteLine($"✅ Đã thêm bài tập '{assignment.Title}' với file: {assignment.UploadedFileUrl}");
                TempData["SuccessMessage"] = "✅ Thêm bài tập thành công!";
                return RedirectToAction(nameof(Assignment));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi thêm bài tập: {ex.Message}");
                ViewBag.Lessons = (await _lessonRepository.FindAsync()).OrderBy(l => l.Title).ToList(); // ✅ Thêm dòng này
                return View("~/Views/Instructor/AssignmentInstructor/AddAssignment.cshtml", assignment);
            }

        }

        /* =====================================================
           ✏️ CHỈNH SỬA BÀI TẬP
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> EditAssignment(Guid id)
        {
            var assignment = await _assignmentRepository.FindByIdAsync(id);
            if (assignment == null) return NotFound();
            ViewBag.Lessons = (await _lessonRepository.FindAsync()).OrderBy(l => l.Title).ToList();
            return View("~/Views/Instructor/AssignmentInstructor/EditAssignment.cshtml", assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment([FromForm] Assignment assignment)
        {
            var existing = await _assignmentRepository.FindByIdAsync(assignment.Id);
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

                await _assignmentRepository.UpdateAsync(existing);

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
            var assignment = await _assignmentRepository.FindByIdAsync(id);
            if (assignment == null)
                return Json(new { success = false, message = "Không tìm thấy bài tập để xóa!" });

            try
            {
                if (!string.IsNullOrEmpty(assignment.UploadedFileUrl))
                    await _supabaseStorage.DeleteFileAsync(assignment.UploadedFileUrl);

                await _assignmentRepository.DeleteByIdAsync(id);

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
            var assignment = await _assignmentRepository.FindByIdAsync(id);
            if (assignment == null)
                return Json(new { success = false, message = "Không tìm thấy bài tập." });

            try
            {
                assignment.IsPublic = !assignment.IsPublic;
                await _assignmentRepository.UpdateAsync(assignment);

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
