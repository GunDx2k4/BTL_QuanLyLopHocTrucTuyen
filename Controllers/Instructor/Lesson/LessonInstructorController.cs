using System.Threading.Tasks;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class LessonInstructorController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;

        public LessonInstructorController(ILessonRepository lessonRepository, ICourseRepository courseRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Lesson()
        {
            var lessons = (await _lessonRepository.FindAsync())
                .OrderByDescending(l => l.BeginTime)
                .ToList();

            return View("~/Views/Instructor/LessonInstructor/Lesson.cshtml", lessons);
        }
        [HttpGet]
        public async Task<IActionResult> DetailLesson(Guid id)
        {

            var lesson = await _lessonRepository.FindByIdAsync(id);

            if (lesson == null)
            {
                return NotFound();
            }
            var allLessons = (await _lessonRepository.FindAsync())
                .OrderBy(l => l.BeginTime)
                .ToList();

            ViewBag.AllLessons = allLessons;

            return View("~/Views/Instructor/LessonInstructor/DetailLesson.cshtml",lesson);
        }

        [HttpGet]
        public async Task<IActionResult> AddLesson()
        {
            // Provide courses for selection if needed in the view
            ViewBag.Courses = (await _courseRepository.FindAsync()).OrderBy(c => c.Name).ToList();
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

            await _lessonRepository.AddAsync(lesson);
            return Json(new { success = true });

        }
        [HttpGet]
        public async Task<IActionResult> EditLesson(Guid id)
        {
            var lesson = await _lessonRepository.FindByIdAsync(id);
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

            var existingLesson = await _lessonRepository.FindByIdAsync(lesson.Id);
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
                await _lessonRepository.UpdateAsync(existingLesson);

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
            var lesson = await _lessonRepository.FindByIdAsync(id);
            if (lesson == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài học!" });
            }

            try
            {
                await _lessonRepository.DeleteByIdAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        


    
    }
}