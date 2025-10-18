using BTL_QuanLyLopHocTrucTuyen.Core.Controllers;
using BTL_QuanLyLopHocTrucTuyen.Models;
using BTL_QuanLyLopHocTrucTuyen.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers
{
    public class UserController(IUserRepository repository) : CrudController<User>(repository)
    {
        
    }
}
