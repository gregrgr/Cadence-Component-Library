using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian")]
public sealed class FootprintVariantsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public FootprintVariantsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(string? search, FootprintStatus? status, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.FootprintVariants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.FootprintName.Contains(search) ||
                x.PackageFamilyCode.Contains(search) ||
                x.VariantType.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(x => x.FootprintName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(new PagedResult<FootprintVariant> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    public IActionResult Create()
    {
        PopulatePackageFamilies();
        return View(new FootprintVariant { Status = FootprintStatus.Draft });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FootprintVariant model)
    {
        if (!ModelState.IsValid)
        {
            PopulatePackageFamilies();
            return View(model);
        }

        model.CreatedBy = User.Identity?.Name;
        _dbContext.FootprintVariants.Add(model);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Footprint Variant created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        PopulatePackageFamilies();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, FootprintVariant model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            PopulatePackageFamilies();
            return View(model);
        }

        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.FootprintName = model.FootprintName;
        item.PackageFamilyCode = model.PackageFamilyCode;
        item.PsmPath = model.PsmPath;
        item.DraPath = model.DraPath;
        item.PadstackSet = model.PadstackSet;
        item.StepPath = model.StepPath;
        item.VariantType = model.VariantType;
        item.Status = model.Status;
        item.UpdatedBy = User.Identity?.Name;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Footprint Variant updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Footprint Variant deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void PopulatePackageFamilies()
    {
        ViewBag.PackageFamilies = new SelectList(
            _dbContext.PackageFamilies.OrderBy(x => x.PackageFamilyCode).ToList(),
            nameof(PackageFamily.PackageFamilyCode),
            nameof(PackageFamily.PackageFamilyCode));
    }
}
