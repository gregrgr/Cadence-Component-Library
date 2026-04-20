using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian,EEReviewer,Purchasing,Viewer")]
public sealed class ManufacturerPartsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ManufacturerPartsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(string? search, string? companyPn, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.ManufacturerParts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Manufacturer.Contains(search) ||
                x.ManufacturerPN.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(companyPn))
        {
            query = query.Where(x => x.CompanyPN == companyPn);
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(x => x.Manufacturer).ThenBy(x => x.ManufacturerPN)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Search = search;
        ViewBag.CompanyPN = companyPn;
        return View(new PagedResult<ManufacturerPart> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.ManufacturerParts
            .Include(x => x.SupplierOffers)
            .FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    public IActionResult Create()
    {
        PopulateCompanyParts();
        return View(new ManufacturerPart { LifecycleStatus = LifecycleStatus.Unknown });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManufacturerPart model)
    {
        if (!CanMutateManufacturerParts())
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            PopulateCompanyParts();
            return View(model);
        }

        model.CreatedBy = User.Identity?.Name;
        _dbContext.ManufacturerParts.Add(model);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Manufacturer Part created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.ManufacturerParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        PopulateCompanyParts();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ManufacturerPart model)
    {
        if (!CanMutateManufacturerParts())
        {
            return Forbid();
        }

        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            PopulateCompanyParts();
            return View(model);
        }

        var item = await _dbContext.ManufacturerParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.CompanyPN = model.CompanyPN;
        item.Manufacturer = model.Manufacturer;
        item.ManufacturerPN = model.ManufacturerPN;
        item.MfgDescription = model.MfgDescription;
        item.PackageCodeRaw = model.PackageCodeRaw;
        item.SourceProvider = model.SourceProvider;
        item.LifecycleStatus = model.LifecycleStatus;
        item.IsApproved = model.IsApproved;
        item.IsPreferred = model.IsPreferred;
        item.ParamJson = model.ParamJson;
        item.VerifiedBy = model.VerifiedBy;
        item.VerifiedAt = model.VerifiedAt;
        item.UpdatedBy = User.Identity?.Name;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Manufacturer Part updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        if (!CanMutateManufacturerParts())
        {
            return Forbid();
        }

        var item = await _dbContext.ManufacturerParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Manufacturer Part deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void PopulateCompanyParts()
    {
        ViewBag.CompanyParts = new SelectList(_dbContext.CompanyParts.OrderBy(x => x.CompanyPN).ToList(), nameof(CompanyPart.CompanyPN), nameof(CompanyPart.CompanyPN));
    }

    private bool CanMutateManufacturerParts()
        => User.IsInRole("Admin") || User.IsInRole("Librarian") || User.IsInRole("Purchasing");
}
