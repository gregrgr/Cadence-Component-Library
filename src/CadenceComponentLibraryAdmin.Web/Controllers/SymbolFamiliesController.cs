using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian,EEReviewer,Viewer")]
public sealed class SymbolFamiliesController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public SymbolFamiliesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.SymbolFamilies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.SymbolFamilyCode.Contains(search) ||
                x.SymbolName.Contains(search) ||
                x.PartClass.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(x => x.SymbolFamilyCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Search = search;
        return View(new PagedResult<SymbolFamily> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.SymbolFamilies.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    public IActionResult Create() => View(new SymbolFamily { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SymbolFamily model)
    {
        if (!CanMutateSymbolFamilies())
        {
            return Forbid();
        }

        if (!ModelState.IsValid) return View(model);

        model.CreatedBy = User.Identity?.Name;
        _dbContext.SymbolFamilies.Add(model);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Symbol Family created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.SymbolFamilies.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, SymbolFamily model)
    {
        if (!CanMutateSymbolFamilies())
        {
            return Forbid();
        }

        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var item = await _dbContext.SymbolFamilies.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.SymbolFamilyCode = model.SymbolFamilyCode;
        item.SymbolName = model.SymbolName;
        item.OlbPath = model.OlbPath;
        item.PartClass = model.PartClass;
        item.GateStyle = model.GateStyle;
        item.PinMapHash = model.PinMapHash;
        item.IsActive = model.IsActive;
        item.UpdatedBy = User.Identity?.Name;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Symbol Family updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        if (!CanMutateSymbolFamilies())
        {
            return Forbid();
        }

        var item = await _dbContext.SymbolFamilies.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Symbol Family deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CanMutateSymbolFamilies()
        => User.IsInRole("Admin") || User.IsInRole("Librarian");
}
