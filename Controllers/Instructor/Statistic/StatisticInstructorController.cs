using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class StatisticInstructorController : BaseInstructorController
    {
        private readonly ApplicationDbContext _context;

        public StatisticInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Statistic(Guid? courseId)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null)
                return Redirect("/Home/Login");

            var redirect = EnsureCourseSelected();
            if (redirect != null) return redirect;

            // 🔹 Lấy danh sách khóa học
            var courses = await _context.Courses
                .Where(c => c.InstructorId.ToString() == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.CourseCount = courses.Count;

            // 🔹 Lấy khóa học hiện tại
            var currentCourseId = courseId ?? GetCurrentCourseId();
            var currentCourse = courses.FirstOrDefault(c => c.Id == currentCourseId);
            ViewBag.CurrentCourseId = currentCourseId;
            ViewBag.CurrentCourseName = currentCourse?.Name ?? "Chưa chọn khóa học";

            if (currentCourse == null)
                return View("~/Views/Instructor/StatisticInstructor/Statistic.cshtml");

            // 🔹 Lấy danh sách bài tập và bài nộp của khóa học
            var assignments = await _context.Assignments
                .Include(a => a.Lesson)
                .Where(a => a.Lesson.CourseId == currentCourseId)
                .ToListAsync();

            var submissions = await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                .Where(s => s.Assignment.Lesson.CourseId == currentCourseId)
                .ToListAsync();

            var studentCount = await _context.Enrollments
                .CountAsync(e => e.CourseId == currentCourseId);

            // 🔹 Thống kê tổng quan
            var submittedCount = submissions.Count;
            var notSubmittedCount = Math.Max(studentCount - submittedCount, 0);
            var lateCount = submissions.Count(s => s.Assignment.AvailableUntil.HasValue && s.SubmittedAt > s.Assignment.AvailableUntil);

            ViewBag.SubmittedCount = submittedCount;
            ViewBag.NotSubmittedCount = notSubmittedCount;
            ViewBag.LateCount = lateCount;

            // 🔹 Dữ liệu chi tiết theo từng bài tập
            var assignmentStats = assignments.Select(a => new
            {
                a.Title,
                Submitted = submissions.Count(s => s.AssignmentId == a.Id),
                NotSubmitted = Math.Max(studentCount - submissions.Count(s => s.AssignmentId == a.Id), 0),
                Late = submissions.Count(s => s.AssignmentId == a.Id && a.AvailableUntil.HasValue && s.SubmittedAt > a.AvailableUntil),
                AvgGrade = submissions.Where(s => s.AssignmentId == a.Id && s.Grade.HasValue).Select(s => s.Grade.Value).DefaultIfEmpty(0).Average()
            }).ToList();

            ViewBag.AssignmentStats = assignmentStats;

            return View("~/Views/Instructor/StatisticInstructor/Statistic.cshtml");
        }


    }
}
