using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class LessonInstructorController : BaseInstructorController
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;

        public LessonInstructorController(ILessonRepository lessonRepository, ICourseRepository courseRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }

        // ✅ Hiển thị danh sách bài học của khóa học đang chọn
        [HttpGet]
        public async Task<IActionResult> Lesson()
        {
            // 🔹 Nếu chưa chọn khóa học thì chuyển về trang chọn
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            // 🔹 Lấy ID khóa học từ Claim
            var courseId = GetCurrentCourseId()!.Value;

            // 🔹 Lấy danh sách bài học của khóa học
            var lessons = (await _lessonRepository.FindAsync())
                .Where(l => l.CourseId == courseId)
                .OrderByDescending(l => l.BeginTime)
                .ToList();

            // 🔹 Lấy thông tin khóa học để hiển thị
            var course = await _courseRepository.FindByIdAsync(courseId);
            ViewBag.CourseName = course?.Name ?? "Khóa học không xác định";
            ViewBag.CourseId = courseId;

            return View("~/Views/Instructor/LessonInstructor/Lesson.cshtml", lessons);
        }

        // ✅ Chi tiết bài học
        [HttpGet]
        public async Task<IActionResult> DetailLesson(Guid id)
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            var courseId = GetCurrentCourseId()!.Value;

            var lesson = await _lessonRepository.FindByIdAsync(id);
            if (lesson == null || lesson.CourseId != courseId)
                return NotFound();

            var allLessons = (await _lessonRepository.FindAsync())
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.BeginTime)
                .ToList();

            ViewBag.AllLessons = allLessons;
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = (await _courseRepository.FindByIdAsync(courseId))?.Name;

            return View("~/Views/Instructor/LessonInstructor/DetailLesson.cshtml", lesson);
        }

        // ✅ Giao diện thêm bài học
        [HttpGet]
        public async Task<IActionResult> AddLesson()
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            var courseId = GetCurrentCourseId()!.Value;
            var course = await _courseRepository.FindByIdAsync(courseId);

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course?.Name;

            return View("~/Views/Instructor/LessonInstructor/AddLesson.cshtml");
        }

        // ✅ Xử lý thêm bài học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson([FromForm] Lesson lesson)
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var courseId = GetCurrentCourseId()!.Value;

            lesson.Id = Guid.NewGuid();
            lesson.CourseId = courseId;
            lesson.Status = Models.Enums.ScheduleStatus.Planned;

            await _lessonRepository.AddAsync(lesson);
            return Json(new { success = true });
        }

        // ✅ Chỉnh sửa bài học
        [HttpGet]
        public async Task<IActionResult> EditLesson(Guid id)
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            var courseId = GetCurrentCourseId()!.Value;

            var lesson = await _lessonRepository.FindByIdAsync(id);
            if (lesson == null || lesson.CourseId != courseId)
                return NotFound();

            ViewBag.CourseId = courseId;
            return View("~/Views/Instructor/LessonInstructor/EditLesson.cshtml", lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson([FromForm] Lesson lesson)
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            if (!ModelState.IsValid)
                return View("~/Views/Instructor/LessonInstructor/EditLesson.cshtml", lesson);

            var existingLesson = await _lessonRepository.FindByIdAsync(lesson.Id);
            if (existingLesson == null)
                return NotFound();

            existingLesson.Title = lesson.Title;
            existingLesson.Content = lesson.Content;
            existingLesson.VideoUrl = lesson.VideoUrl;
            existingLesson.BeginTime = lesson.BeginTime;
            existingLesson.EndTime = lesson.EndTime;

            await _lessonRepository.UpdateAsync(existingLesson);

            TempData["SuccessMessage"] = "✅ Cập nhật bài học thành công!";
            return RedirectToAction(nameof(Lesson));
        }

        // ✅ Xóa bài học
        [HttpDelete]
        public async Task<IActionResult> DeleteLesson(Guid id)
        {
            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            var lesson = await _lessonRepository.FindByIdAsync(id);
            if (lesson == null)
                return Json(new { success = false, message = "Không tìm thấy bài học!" });

            await _lessonRepository.DeleteByIdAsync(id);
            return Json(new { success = true });
        }
    }
}
