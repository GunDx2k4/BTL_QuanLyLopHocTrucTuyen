using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Manager
{
    [UserPermissionAuthorize(UserPermission.ManagerUser)]
    public class ManagerController(ITenantRepository tenantRepository) : Controller
    {
        // GET: ManagerController
        public async Task<ActionResult> Index()
        {
            var userId = User.GetUserId();
            var tenant = await tenantRepository.FindTenantByOwnerIdAsync(userId);
            if (tenant == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return View("Index", "Home");
            }
            var studentCount = tenant.Users.Count(u => u.Role.Permissions.HasPermission(UserPermission.StudentUser));
            var instructorCount = tenant.Users.Count(u => u.Role.Permissions.HasPermission(UserPermission.InstructorUser));
            var courseCount = tenant.Courses.Count(c => c.EndTime.ToUniversalTime() > DateTime.UtcNow);
            ViewBag.TenantId = tenant.Id;
            ViewBag.TenantName = tenant.Name;
            ViewBag.TenantPlan = tenant.Plan;
            ViewBag.TenantEndTime = tenant.EndTime == null ? "Không giới hạn" : tenant.EndTime.Value.ToString("dd/MM/yyyy");
            ViewBag.StudentCount = studentCount;
            ViewBag.InstructorCount = instructorCount;
            ViewBag.CourseCount = courseCount;
            return View();
        }

        public IActionResult StudentManager()
        {
            return View();
        }

        public IActionResult InstructorManager()
        {
            return View();
        }

        public IActionResult CourseManager()
        {
            return View();
        }   

    }
}
