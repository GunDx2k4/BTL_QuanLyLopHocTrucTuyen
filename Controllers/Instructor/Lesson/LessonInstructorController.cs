using System.Threading.Tasks;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class LessonInstructorController : Controller
    {   
        private readonly SqlServerDbContext _context;
        public LessonInstructorController(SqlServerDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Lesson()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Course)
                .OrderByDescending(l => l.BeginTime)
                .ToListAsync();
            return View("~/Views/Instructor/LessonInstructor/Lesson.cshtml", lessons);
        }
        [HttpGet]
        public async Task<IActionResult> DetailLesson(Guid id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Materials)
                .Include(l => l.Assignments)
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }
             var allLessons = await _context.Lessons
                .OrderBy(l => l.BeginTime)
                .ToListAsync();

            ViewBag.AllLessons = allLessons;

            return View("~/Views/Instructor/LessonInstructor/DetailLesson.cshtml",lesson);
        }

        [HttpGet]
        public IActionResult AddLesson()
        {
            return View("~/Views/Instructor/LessonInstructor/AddLesson.cshtml");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson([FromForm] Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { succsess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            lesson.Id = Guid.NewGuid();
            lesson.Status = Models.Enums.ScheduleStatus.Planned;

            _context.Add(lesson);
            await _context.SaveChangesAsync();
            return Json(new { success = true });

        }
        [HttpGet]
        public async Task<IActionResult> EditLesson(Guid id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View("~/Views/Instructor/LessonInstructor/EditLesson.cshtml", lesson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson([FromForm] Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Instructor/LessonInstructor/EditLesson.cshtml", lesson);
            }

            var existingLesson = await _context.Lessons.FindAsync(lesson.Id);
            if (existingLesson == null)
            {
                return NotFound();
            }

            // Cập nhật các trường được phép chỉnh sửa
            existingLesson.Title = lesson.Title;
            existingLesson.Content = lesson.Content;
            existingLesson.VideoUrl = lesson.VideoUrl;
            existingLesson.BeginTime = lesson.BeginTime;
            existingLesson.EndTime = lesson.EndTime;

            try
            {
                _context.Update(existingLesson);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Cập nhật bài học thành công!";
                return RedirectToAction(nameof(Lesson));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "❌ Lỗi khi lưu thay đổi: " + ex.Message);
                return View("~/Views/Instructor/LessonInstructor/EditLesson.cshtml", lesson);
            }
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteLesson(Guid id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài học!" });
            }

            try
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    
    }
}