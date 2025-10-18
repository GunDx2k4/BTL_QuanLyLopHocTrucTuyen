using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers.API
{
    [Route("api/course")]
    public class CourseApiController(ICourseRepository repository) : CrudApiController<Course>(repository)
    {
        
    }
}
