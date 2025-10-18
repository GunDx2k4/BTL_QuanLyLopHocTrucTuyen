using System.ComponentModel.Design;
using BTL_QuanLyLopHocTrucTuyen.Core.Models;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Core.Controllers
{
    [ApiController]
    public abstract class CrudApiController<T>(IEntityRepository<T> repository) : ControllerBase where T : IEntity
    {
        public IActionResult EntityResponse(T? entity)
        {
            if (entity is null)
            {
                return NotFound();
            }
            return Ok(entity);
        }
        
        [HttpGet]
        public virtual async Task<IActionResult> GetAsync()
        {
            var entities = await repository.FindAsync();
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public virtual async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var entity = await repository.FindByIdAsync(id);
            return EntityResponse(entity);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddAsync([FromBody] T entity)
        {
            var addedEntity = await repository.AddAsync(entity);

            return EntityResponse(addedEntity);
        }

        [HttpPut]
        public virtual async Task<IActionResult> UpdateAsync([FromBody] T entity)
        {
            var existingEntity = await repository.FindByIdAsync(entity.Id);
            if (existingEntity is null)
            {
                return NotFound();
            }

            // Update the entity properties
            existingEntity = entity;

            var updatedEntity = await repository.UpdateAsync(existingEntity);
            return Ok(new { success = true, updated = updatedEntity });
        }

        [HttpDelete("{id:guid}")]
        public virtual async Task<IActionResult> DeleteByIdAsync(Guid id)
        {
            var deletedCount = await repository.DeleteByIdAsync(id);
            if (deletedCount == 0)
            {
                return NotFound();
            }
            return Ok(new { success = true, deleted = deletedCount });
        }

        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAllAsync()
        {
            var deletedCount = await repository.DeleteAllAsync();
            if (deletedCount == 0)
            {
                return NotFound();
            }
            return Ok(new { success = true, deleted = deletedCount });
        }
    }
}
