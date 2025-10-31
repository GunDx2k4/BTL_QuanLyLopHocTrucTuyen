using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Admin
{
    [UserPermissionAuthorize(UserPermission.AdminUser)]
    public class AdminController : Controller
    {
        // GET: AdminController
        public ActionResult Index()
        {
            ViewBag.UserName = HttpContext.User.Identity?.Name;
            return View();
        }

        public IActionResult CreateUser()
        {
            return View();
        }

    }
}
