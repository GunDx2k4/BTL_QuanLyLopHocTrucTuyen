using System.Threading.Tasks;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Core.Controllers
{
    public abstract class CrudController<T>(IEntityRepository<T> repository) : Controller where T : IEntity
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("create")]
        public IActionResult CreateEntity()
        {
            return View("Create");
        }

        [HttpGet("manager")]
        public async Task<IActionResult> ManageEntity()
        {
            var entities = await repository.FindAsync();
            return View("Manage", entities);
        }

        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> DetailsEntity(Guid id)
        {
            var entity = await repository.FindByIdAsync(id);
            if (entity is null)
            {
                return NotFound();
            }
            return View("Details", entity);
        }

    }
}
