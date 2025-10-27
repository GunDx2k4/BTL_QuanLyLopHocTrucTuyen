using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Data;
using BTL_QuanLyLopHocTrucTuyen.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    [Route("Instructor/[action]")]
    public class MaterialInstructorController : Controller
    {
        private readonly SqlServerDbContext _context;
        public MaterialInstructorController(SqlServerDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Material()
        {
            var material = await _context.Materials
                .Include(m => m.Lesson)
                .Include(m => m.Uploader)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            return View("~/Views/Instructor/MaterialInstructor/Material.cshtml", material);
        }

        [HttpGet]
        public async Task<IActionResult> AddMaterial(Guid? lessonId)
        {
            ViewBag.Lessons = await _context.Lessons
                .OrderBy(l => l.Title)
                .ToListAsync();

            var material = new Material
            {
                LessonId = lessonId ?? Guid.Empty
            };

            return View("~/Views/Instructor/MaterialInstructor/AddMaterial.cshtml", material);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial([FromForm] Material material)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { succsess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            material.Id = Guid.NewGuid();

            _context.Add(material);
            await _context.SaveChangesAsync();
            return Json(new { success = true });

        }
        [HttpGet]
        public async Task<IActionResult> EditMaterial(Guid id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            return View("~/Views/Instructor/MaterialInstructor/EditMaterial.cshtml", material);
        
        }
    }
}