using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;


namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class GradeInstructorController : BaseInstructorController
    {
        [HttpGet]
        public IActionResult Grade()
        {
            return View("~/Views/Instructor/GradeInstructor/Grade.cshtml");
        }
        [HttpGet]
        public IActionResult DetailGrade(Guid id)
        {
            // sau này có thể lấy chi tiết điểm từ DB theo id
            return View("~/Views/Instructor/GradeInstructor/DetailGrade.cshtml");
        }

    }
}
