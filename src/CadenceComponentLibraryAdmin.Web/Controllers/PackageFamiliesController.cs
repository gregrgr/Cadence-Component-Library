using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian,EEReviewer,Viewer")]
public sealed class PackageFamiliesController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPackageFamilyService _packageFamilyService;

    public PackageFamiliesController(ApplicationDbContext dbContext, IPackageFamilyService packageFamilyService)
    {
        _dbContext = dbContext;
        _packageFamilyService = packageFamilyService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.PackageFamilies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.PackageFamilyCode.Contains(search) ||
                x.PackageSignature.Contains(search) ||
                x.MountType.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(x => x.PackageFamilyCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Search = search;
        return View(new PagedResult<PackageFamily> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.PackageFamilies
            .Include(x => x.FootprintVariants)
            .FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    public IActionResult Create() => View(new PackageFamily());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PackageFamily model)
    {
        if (!CanMutatePackageFamilies())
        {
            return Forbid();
        }

        if (!ModelState.IsValid) return View(model);

        var result = await _packageFamilyService.PrepareForSaveAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        model.CreatedBy = User.Identity?.Name;
        _dbContext.PackageFamilies.Add(model);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Package Family created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.PackageFamilies.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PackageFamily model)
    {
        if (!CanMutatePackageFamilies())
        {
            return Forbid();
        }

        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var result = await _packageFamilyService.PrepareForSaveAsync(model, id);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        var item = await _dbContext.PackageFamilies.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.PackageFamilyCode = model.PackageFamilyCode;
        item.MountType = model.MountType;
        item.LeadCount = model.LeadCount;
        item.BodyLmm = model.BodyLmm;
        item.BodyWmm = model.BodyWmm;
        item.PitchMm = model.PitchMm;
        item.EPLmm = model.EPLmm;
        item.EPWmm = model.EPWmm;
        item.DensityLevel = model.DensityLevel;
        item.PackageStd = model.PackageStd;
        item.Notes = model.Notes;
        item.PackageSignature = model.PackageSignature;
        item.UpdatedBy = User.Identity?.Name;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Package Family updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        if (!CanMutatePackageFamilies())
        {
            return Forbid();
        }

        var item = await _dbContext.PackageFamilies.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Package Family deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CanMutatePackageFamilies()
        => User.IsInRole("Admin") || User.IsInRole("Librarian");
}
