using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API
{
    [Route("api/tenants")]
    public class TenantApiController(ITenantRepository repository) : CrudApiController<Tenant>(repository)
    {
        public override async Task<IActionResult> AddAsync([FromBody] Tenant entity)
        {
            return await base.AddAsync(entity);
        }

        public override async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return await base.GetByIdAsync(id);
        }

        public override async Task<IActionResult> GetAsync()
        {
            return await base.GetAsync();
        }
        
    }
}
