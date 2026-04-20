using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadenceComponentLibraryAdmin.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Users", new { area = "Admin" });
    }
}
