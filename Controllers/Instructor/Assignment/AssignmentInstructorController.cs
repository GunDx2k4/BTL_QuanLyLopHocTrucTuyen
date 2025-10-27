using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc;
namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class AssignmentInstructorController : Controller
    {
        private readonly SqlServerDbContext _context;
        public AssignmentInstructorController(SqlServerDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Assignment()
        {
            // var assignment=await _context.Assignments
            //     .Include(a => a.Lesson)
            //     .OrderByDescending(a => a.DueDate)
            //     .ToListAsync();
            return View("~/Views/Instructor/AssignmentInstructor/Assignment.cshtml");
        }

    }
}