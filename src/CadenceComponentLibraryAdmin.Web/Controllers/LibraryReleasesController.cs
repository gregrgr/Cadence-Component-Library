using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class LibraryReleasesController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILibraryReleaseService _libraryReleaseService;

    public LibraryReleasesController(ApplicationDbContext dbContext, ILibraryReleaseService libraryReleaseService)
    {
        _dbContext = dbContext;
        _libraryReleaseService = libraryReleaseService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        var query = _dbContext.LibraryReleases
            .AsNoTracking()
            .OrderByDescending(x => x.ReleaseDate)
            .ThenByDescending(x => x.Id);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Draft = await _libraryReleaseService.BuildDraftAsync();
        return View(new PagedResult<CadenceComponentLibraryAdmin.Domain.Entities.LibraryRelease> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.LibraryReleases
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDraft(string? releaseNote)
    {
        var releasedBy = User.Identity?.Name ?? "system";
        await _libraryReleaseService.CreateDraftAsync(releasedBy, releaseNote);
        TempData["SuccessMessage"] = "Library Release draft created or refreshed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Release(long id)
    {
        var releasedBy = User.Identity?.Name ?? "system";
        var result = await _libraryReleaseService.ReleaseAsync(id, releasedBy);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["SuccessMessage"] = "Library Release marked as Released.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(long id)
    {
        var item = await _dbContext.LibraryReleases.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.Status = ReleaseStatus.Archived;
            item.UpdatedBy = User.Identity?.Name ?? "system";
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Library Release archived.";
        }

        return RedirectToAction(nameof(Index));
    }
}
