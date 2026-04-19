using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public sealed class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly FileStorageOptions _fileStorage;

    public HomeController(IConfiguration configuration, IOptions<FileStorageOptions> fileStorage)
    {
        _configuration = configuration;
        _fileStorage = fileStorage.Value;
    }

    public IActionResult Index()
    {
        var model = new HomeIndexViewModel
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            ConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
            AppDataRoot = _fileStorage.AppDataRoot ?? string.Empty,
            LibraryRoot = _fileStorage.LibraryRoot ?? string.Empty,
            LogRoot = _fileStorage.LogRoot ?? string.Empty,
            Cards =
            [
                new DashboardCardViewModel
                {
                    Title = "Database",
                    Value = "SQL Server",
                    Description = "Entity Framework Core and Identity are configured to target CadenceComponentLibrary."
                },
                new DashboardCardViewModel
                {
                    Title = "Roles",
                    Value = "6",
                    Description = "Admin, Librarian, EEReviewer, Purchasing, Designer, and Viewer are seeded on startup."
                },
                new DashboardCardViewModel
                {
                    Title = "Admin",
                    Value = "admin@local.test",
                    Description = "The initial administrator account is created automatically."
                }
            ]
        };

        return View(model);
    }
}
