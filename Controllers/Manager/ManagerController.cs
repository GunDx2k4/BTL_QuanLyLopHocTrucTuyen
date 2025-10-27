using BTL_QuanLyLopHocTrucTuyen.Authorizations;
using BTL_QuanLyLopHocTrucTuyen.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Manager
{
    [UserPermissionAuthorize(UserPermission.Manager)]
    public class ManagerController : Controller
    {
        // GET: ManagerController
        public ActionResult Index()
        {
            return View();
        }

    }
}
