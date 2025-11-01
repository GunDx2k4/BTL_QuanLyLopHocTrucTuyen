using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Helpers;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Manager
{
    [UserPermissionAuthorize(UserPermission.ManagerUser)]
    public class ManagerController(ITenantRepository tenantRepository, IMemoryCache memoryCache) : Controller
    {
        // GET: ManagerController
        public async Task<ActionResult> Index()
        {
            var userId = User.GetUserId();
            var tenant = await tenantRepository.FindTenantByOwnerIdAsync(userId);
            if (tenant == null)
            {
                memoryCache.Remove(userId);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return View("Index", "Home");
            }

            // defensive null checks to satisfy nullable analysis
            var studentCount = tenant.Users?.Count(u => u?.Role != null && u.Role.Permissions.HasPermission(UserPermission.StudentUser)) ?? 0;
            var instructorCount = tenant.Users?.Count(u => u?.Role != null && u.Role.Permissions.HasPermission(UserPermission.InstructorUser)) ?? 0;
            var courseCount = tenant.Courses?.Count(c => c != null && c.EndTime.ToUniversalTime() > DateTime.UtcNow) ?? 0;

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
