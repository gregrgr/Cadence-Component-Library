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

[Authorize(Roles = "Admin,Librarian,EEReviewer,Purchasing")]
public sealed class AlternatesController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPartAlternateService _partAlternateService;

    public AlternatesController(ApplicationDbContext dbContext, IPartAlternateService partAlternateService)
    {
        _dbContext = dbContext;
        _partAlternateService = partAlternateService;
    }

    public async Task<IActionResult> Index(string? companyPn, AlternateLevel? altLevel, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.PartAlternates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(companyPn))
        {
            query = query.Where(x => x.SourceCompanyPN.Contains(companyPn) || x.TargetCompanyPN.Contains(companyPn));
        }

        if (altLevel.HasValue)
        {
            query = query.Where(x => x.AltLevel == altLevel.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.SourceCompanyPN)
            .ThenBy(x => x.TargetCompanyPN)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CompanyPN = companyPn;
        ViewBag.AltLevel = altLevel;
        return View(new PagedResult<PartAlternate>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.PartAlternates.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    public IActionResult Create()
    {
        PopulateCompanyParts();
        return View(new PartAlternate
        {
            AltLevel = AlternateLevel.B,
            NeedEEReviewYN = true,
            NeedLayoutReviewYN = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartAlternate model)
    {
        var ruleResult = await _partAlternateService.ValidateAsync(model);
        if (!ModelState.IsValid || !ruleResult.Succeeded)
        {
            foreach (var error in ruleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            PopulateCompanyParts();
            return View(model);
        }

        model.CreatedBy = User.Identity?.Name;
        _dbContext.PartAlternates.Add(model);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Alternate relation created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.PartAlternates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        PopulateCompanyParts();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PartAlternate model)
    {
        if (id != model.Id) return NotFound();

        var ruleResult = await _partAlternateService.ValidateAsync(model);
        if (!ModelState.IsValid || !ruleResult.Succeeded)
        {
            foreach (var error in ruleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            PopulateCompanyParts();
            return View(model);
        }

        var item = await _dbContext.PartAlternates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.SourceCompanyPN = model.SourceCompanyPN;
        item.TargetCompanyPN = model.TargetCompanyPN;
        item.AltLevel = model.AltLevel;
        item.SameFootprintYN = model.SameFootprintYN;
        item.SameSymbolYN = model.SameSymbolYN;
        item.NeedEEReviewYN = model.NeedEEReviewYN;
        item.NeedLayoutReviewYN = model.NeedLayoutReviewYN;
        item.Notes = model.Notes;
        item.UpdatedBy = User.Identity?.Name;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Alternate relation updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id)
    {
        var item = await _dbContext.PartAlternates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.ApprovedBy = User.Identity?.Name;
            item.ApprovedAt = DateTime.UtcNow;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Alternate relation approved.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void PopulateCompanyParts()
    {
        var items = _dbContext.CompanyParts
            .OrderBy(x => x.CompanyPN)
            .Select(x => new { x.CompanyPN })
            .ToList();

        ViewBag.CompanyParts = new SelectList(items, "CompanyPN", "CompanyPN");
    }
}
