using Microsoft.AspNetCore.Mvc;
namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Instructor
{
    [Route("Instructor")]
    public class HomeInstructorController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Instructor/Index.cshtml");
        }
    }

}
