using Microsoft.AspNetCore.Mvc;
namespace BTL_QuanLyLopHocTrucTuyen.Controllers.Instructor
{
    
    public class HomeInstructorController : Controller
    {
        [Route("Instructor/[action]")]
        public IActionResult Home()
        {
            return View("~/Views/Instructor/HomeInstructor/Home.cshtml");
        }
    }

}
