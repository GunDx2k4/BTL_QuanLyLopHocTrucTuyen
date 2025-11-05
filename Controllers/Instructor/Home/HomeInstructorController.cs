using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;


namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Instructor
{
    [Route("Instructor")]
    public class HomeInstructorController : BaseInstructorController
    {
        private readonly ApplicationDbContext _context;

        public HomeInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instructorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorIdClaim))
                return Redirect("/Home/Login");

            var instructorId = Guid.Parse(instructorIdClaim);

            // ===============================
            // 🔹 1. Khóa học gần đây
            // ===============================
            var cookie = Request.Cookies["RecentCourses"];
            List<Guid> recentIds = new();
            if (!string.IsNullOrEmpty(cookie))
            {
                recentIds = cookie.Split(',')
                    .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .ToList();
            }

            var recentCourses = await _context.Courses
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == instructorId && recentIds.Contains(c.Id))
                .ToListAsync();

            ViewBag.RecentCourses = recentCourses
                .OrderBy(c => recentIds.IndexOf(c.Id))
                .ToList();

            // ===============================
            // 🔹 2. Lịch tuần này
            // ===============================
            DateTime today = DateTime.Today;
            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Thứ 2
            DateTime endOfWeek = startOfWeek.AddDays(7);

            var lessonsThisWeek = await _context.Lessons
                .Include(l => l.Course)
                .Where(l => l.Course != null &&
                            l.Course.InstructorId == instructorId &&
                            l.BeginTime >= startOfWeek &&
                            l.BeginTime < endOfWeek)
                .OrderBy(l => l.BeginTime)
                .ToListAsync();

            var weekSchedule = new Dictionary<string, Dictionary<string, List<Lesson>>>();
            string[] days = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN" };
            string[] times = { "Sáng", "Chiều", "Tối" };

            foreach (var day in days)
            {
                weekSchedule[day] = new Dictionary<string, List<Lesson>>();
                foreach (var time in times)
                    weekSchedule[day][time] = new List<Lesson>();
            }

            foreach (var lesson in lessonsThisWeek)
            {
                string dayOfWeek = lesson.BeginTime.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Thứ 2",
                    DayOfWeek.Tuesday => "Thứ 3",
                    DayOfWeek.Wednesday => "Thứ 4",
                    DayOfWeek.Thursday => "Thứ 5",
                    DayOfWeek.Friday => "Thứ 6",
                    DayOfWeek.Saturday => "Thứ 7",
                    _ => "CN"
                };

                string session = lesson.BeginTime.Hour switch
                {
                    >= 5 and < 12 => "Sáng",
                    >= 12 and < 18 => "Chiều",
                    _ => "Tối"
                };

                weekSchedule[dayOfWeek][session].Add(lesson);
            }

            ViewBag.WeekSchedule = weekSchedule;

            // ===============================
            // 🔹 3. Sự kiện sắp diễn ra / kết thúc
            // ===============================
            var now = DateTime.Now;
            var todayEnd = today.AddDays(1);

            // 🔸 Lấy tất cả khóa học mà giảng viên đang dạy
            var courseIds = await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Select(c => c.Id)
                .ToListAsync();

            // 🔸 Lấy bài học trong ngày hôm nay
            var lessonsToday = await _context.Lessons
                .Where(l => l.CourseId.HasValue &&
                            courseIds.Contains(l.CourseId.Value) &&
                            l.BeginTime.Date == today)
                .ToListAsync();

            // 🔸 Bài học sắp diễn ra (chưa bắt đầu)
            var upcomingLessons = lessonsToday
                .Where(l => l.BeginTime > now)
                .OrderBy(l => l.BeginTime)
                .Take(5)
                .ToList();

            // 🔸 Bài học sắp kết thúc (đang diễn ra và sắp hết)
            var endingLessons = lessonsToday
                .Where(l => l.EndTime > now && l.EndTime <= todayEnd)
                .OrderBy(l => l.EndTime)
                .Take(5)
                .ToList();

            // 🔸 Lấy bài tập trong ngày (liên quan đến bài học cùng khóa)
            var lessonIds = lessonsToday.Select(l => l.Id).ToList();

            var assignmentsToday = await _context.Assignments
                .Include(a => a.Lesson)
                .ThenInclude(l => l.Course)
                .Where(a => lessonIds.Contains(a.LessonId) &&
                (
                    (a.AvailableFrom.HasValue && a.AvailableFrom.Value >= today.AddDays(-1)) || // hôm qua trở đi
                    (a.AvailableUntil.HasValue && a.AvailableUntil.Value >= today && a.AvailableUntil.Value < today.AddDays(2)) // hôm nay + ngày mai
                ))
                .ToListAsync();

            // 🔸 Lọc các bài tập sắp bắt đầu
            var upcomingAssignments = assignmentsToday
                .Where(a => a.AvailableFrom.HasValue && a.AvailableFrom > now)
                .OrderBy(a => a.AvailableFrom)
                .Take(10)
                .ToList();
            var endingAssignments = assignmentsToday
                .Where(a => a.AvailableUntil.HasValue && a.AvailableUntil > now && a.AvailableUntil <= todayEnd)
                .OrderBy(a => a.AvailableUntil)
                .Take(5)
                .ToList();

            // 🔸 Phân loại theo Type (chuỗi)
           List<Assignment> FilterByType(List<Assignment> list, string type)
                => list.Where(a => a.Type == type).ToList();

            var upcomingTests = FilterByType(upcomingAssignments, "Bài kiểm tra");
            var upcomingExams = FilterByType(upcomingAssignments, "Bài thi");

            var endingTests = FilterByType(endingAssignments, "Bài kiểm tra");
            var endingExams = FilterByType(endingAssignments, "Bài thi");
            // 🔸 Bài tập sắp kết thúc
            

            // ✅ Truyền dữ liệu sang View
            ViewBag.UpcomingLessons = upcomingLessons;
            ViewBag.EndingLessons = endingLessons;
            ViewBag.UpcomingTests = upcomingTests;
            ViewBag.UpcomingExams = upcomingExams;
            ViewBag.EndingAssignments = endingAssignments;
            ViewBag.EndingTests = endingTests;
            ViewBag.EndingExams = endingExams;
            return View("~/Views/Instructor/Index.cshtml");
        }
    }
}
