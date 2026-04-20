using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian,EEReviewer,Purchasing,Designer,Viewer")]
public sealed class CompanyPartsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICompanyPartService _companyPartService;

    public CompanyPartsController(ApplicationDbContext dbContext, ICompanyPartService companyPartService)
    {
        _dbContext = dbContext;
        _companyPartService = companyPartService;
    }

    public async Task<IActionResult> Index(string? search, ApprovalStatus? approvalStatus, LifecycleStatus? lifecycleStatus, string? packageFamilyCode, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.CompanyParts
            .Include(x => x.SymbolFamily)
            .Include(x => x.PackageFamily)
            .Include(x => x.DefaultFootprint)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.CompanyPN.Contains(search) ||
                x.Description.Contains(search) ||
                (x.ValueNorm != null && x.ValueNorm.Contains(search)) ||
                x.PartClass.Contains(search));
        }

        if (approvalStatus.HasValue)
        {
            query = query.Where(x => x.ApprovalStatus == approvalStatus.Value);
        }

        if (lifecycleStatus.HasValue)
        {
            query = query.Where(x => x.LifecycleStatus == lifecycleStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(packageFamilyCode))
        {
            query = query.Where(x => x.PackageFamilyCode == packageFamilyCode);
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(x => x.CompanyPN)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Search = search;
        ViewBag.ApprovalStatus = approvalStatus;
        ViewBag.LifecycleStatus = lifecycleStatus;
        ViewBag.PackageFamilyCode = packageFamilyCode;
        return View(new PagedResult<CompanyPart>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.CompanyParts
            .Include(x => x.SymbolFamily)
            .Include(x => x.PackageFamily)
            .Include(x => x.DefaultFootprint)
            .Include(x => x.ManufacturerParts)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var relatedAlternates = await _dbContext.PartAlternates
            .Where(x => !x.IsDeleted && (x.SourceCompanyPN == item.CompanyPN || x.TargetCompanyPN == item.CompanyPN))
            .OrderBy(x => x.SourceCompanyPN)
            .ThenBy(x => x.TargetCompanyPN)
            .ToListAsync();

        var relatedCompanyPns = relatedAlternates
            .SelectMany(x => new[] { x.SourceCompanyPN, x.TargetCompanyPN })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var descriptions = await _dbContext.CompanyParts
            .Where(x => relatedCompanyPns.Contains(x.CompanyPN))
            .ToDictionaryAsync(x => x.CompanyPN, x => x.Description);

        AlternateListItemViewModel BuildAlternateListItem(PartAlternate alternate) => new()
        {
            Alternate = alternate,
            SourceDescription = descriptions.GetValueOrDefault(alternate.SourceCompanyPN),
            TargetDescription = descriptions.GetValueOrDefault(alternate.TargetCompanyPN)
        };

        var model = new CompanyPartDetailsViewModel
        {
            CompanyPart = item,
            ApprovedManufacturerParts = item.ManufacturerParts
                .Where(x => x.IsApproved)
                .OrderBy(x => x.Manufacturer)
                .ThenBy(x => x.ManufacturerPN)
                .ToList(),
            SourceAlternates = relatedAlternates
                .Where(x => x.SourceCompanyPN == item.CompanyPN)
                .Select(BuildAlternateListItem)
                .ToList(),
            TargetAlternates = relatedAlternates
                .Where(x => x.TargetCompanyPN == item.CompanyPN)
                .Select(BuildAlternateListItem)
                .ToList()
        };

        return View(model);
    }

    public IActionResult Create()
    {
        PopulateLookups();
        return View(new CompanyPart
        {
            ApprovalStatus = ApprovalStatus.New,
            LifecycleStatus = LifecycleStatus.Unknown,
            PreferredYN = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyPart model)
    {
        if (!CanMutateCompanyParts())
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            PopulateLookups();
            return View(model);
        }

        var ruleResult = await _companyPartService.ValidateApprovalAsync(model);
        if (!ruleResult.Succeeded)
        {
            foreach (var error in ruleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            PopulateLookups();
            return View(model);
        }

        model.CreatedBy = User.Identity?.Name;
        _dbContext.CompanyParts.Add(model);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Company Part created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        PopulateLookups();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, CompanyPart model)
    {
        if (!CanMutateCompanyParts())
        {
            return Forbid();
        }

        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            PopulateLookups();
            return View(model);
        }

        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        var changedBy = User.Identity?.Name ?? "system";
        var ruleResult = await _companyPartService.ApplyEditRulesAsync(item, model, changedBy);
        if (!ruleResult.Succeeded)
        {
            foreach (var error in ruleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            PopulateLookups();
            return View(model);
        }

        item.CompanyPN = model.CompanyPN;
        item.PartClass = model.PartClass;
        item.Description = model.Description;
        item.ValueNorm = model.ValueNorm;
        item.SymbolFamilyCode = model.SymbolFamilyCode;
        item.PackageFamilyCode = model.PackageFamilyCode;
        item.DefaultFootprintName = model.DefaultFootprintName;
        item.ApprovalStatus = model.ApprovalStatus;
        item.LifecycleStatus = model.LifecycleStatus;
        item.AltGroup = model.AltGroup;
        item.PreferredYN = model.PreferredYN;
        item.HeightMaxMm = model.HeightMaxMm;
        item.TempRange = model.TempRange;
        item.RoHS = model.RoHS;
        item.REACHStatus = model.REACHStatus;
        item.DatasheetUrl = model.DatasheetUrl;
        item.UpdatedBy = changedBy;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Company Part updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        if (!CanMutateCompanyParts())
        {
            return Forbid();
        }

        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Company Part deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void PopulateLookups()
    {
        ViewBag.SymbolFamilies = new SelectList(_dbContext.SymbolFamilies.OrderBy(x => x.SymbolFamilyCode).ToList(), nameof(SymbolFamily.SymbolFamilyCode), nameof(SymbolFamily.SymbolFamilyCode));
        ViewBag.PackageFamilies = new SelectList(_dbContext.PackageFamilies.OrderBy(x => x.PackageFamilyCode).ToList(), nameof(PackageFamily.PackageFamilyCode), nameof(PackageFamily.PackageFamilyCode));
        ViewBag.FootprintVariants = new SelectList(_dbContext.FootprintVariants.OrderBy(x => x.FootprintName).ToList(), nameof(FootprintVariant.FootprintName), nameof(FootprintVariant.FootprintName));
    }

    private bool CanMutateCompanyParts()
        => User.IsInRole("Admin") || User.IsInRole("Librarian") || User.IsInRole("EEReviewer") || User.IsInRole("Purchasing");
}
