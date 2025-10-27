using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BTL_QuanLyLopHocTrucTuyen.Models.ViewModels;
using BTL_QuanLyLopHocTrucTuyen.Helpers;

namespace BTL_QuanLyLopHocTrucTuyen.Controllers;

public class HomeController : Controller
{

    public async Task<IActionResult> Index()
    {
        return await this.RedirectToHomePage();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> Login()
    {
        if (HttpContext.User.Identity!.IsAuthenticated)
        {
            return await this.RedirectToHomePage();
        }

        return View();
    }

    public async Task<IActionResult> Register()
    {
        if (HttpContext.User.Identity!.IsAuthenticated)
        {
            return await this.RedirectToHomePage();
        }
        return View();
    }
}
