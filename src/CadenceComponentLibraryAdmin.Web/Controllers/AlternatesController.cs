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

[Authorize(Roles = "Admin,Librarian,EEReviewer,Viewer")]
public sealed class AlternatesController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IChangeLogService _changeLogService;
    private readonly IPartAlternateService _partAlternateService;

    public AlternatesController(
        ApplicationDbContext dbContext,
        IPartAlternateService partAlternateService,
        IChangeLogService changeLogService)
    {
        _dbContext = dbContext;
        _partAlternateService = partAlternateService;
        _changeLogService = changeLogService;
    }

    public async Task<IActionResult> Index(string? sourceCompanyPN, string? targetCompanyPN, AlternateLevel? altLevel, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.PartAlternates
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(sourceCompanyPN))
        {
            query = query.Where(x => x.SourceCompanyPN.Contains(sourceCompanyPN));
        }

        if (!string.IsNullOrWhiteSpace(targetCompanyPN))
        {
            query = query.Where(x => x.TargetCompanyPN.Contains(targetCompanyPN));
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

        var companyPns = items
            .SelectMany(x => new[] { x.SourceCompanyPN, x.TargetCompanyPN })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var descriptions = await _dbContext.CompanyParts
            .Where(x => companyPns.Contains(x.CompanyPN))
            .ToDictionaryAsync(x => x.CompanyPN, x => x.Description);

        var listItems = items
            .Select(item => new AlternateListItemViewModel
            {
                Alternate = item,
                SourceDescription = descriptions.GetValueOrDefault(item.SourceCompanyPN),
                TargetDescription = descriptions.GetValueOrDefault(item.TargetCompanyPN)
            })
            .ToList();

        return View(new AlternatesIndexViewModel
        {
            SourceCompanyPN = sourceCompanyPN,
            TargetCompanyPN = targetCompanyPN,
            AltLevel = altLevel,
            Results = new PagedResult<AlternateListItemViewModel>
            {
                Items = listItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.PartAlternates
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        return View(await BuildAlternateListItemAsync(item));
    }

    public IActionResult Create(string? sourceCompanyPN = null)
    {
        PopulateCompanyParts();
        return View(new PartAlternate
        {
            SourceCompanyPN = sourceCompanyPN ?? string.Empty,
            AltLevel = AlternateLevel.B,
            NeedEEReviewYN = true,
            NeedLayoutReviewYN = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartAlternate model)
    {
        if (!CanEditAlternates())
        {
            return Forbid();
        }

        await _partAlternateService.PrepareForSaveAsync(model);
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
        await WriteAuditAsync(
            model.SourceCompanyPN,
            model.TargetCompanyPN,
            ChangeType.NewPart,
            "None",
            "PendingApproval",
            "Alternate relation created.");
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Alternate relation created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.PartAlternates
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        PopulateCompanyParts();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PartAlternate model)
    {
        if (!CanEditAlternates())
        {
            return Forbid();
        }

        if (id != model.Id) return NotFound();

        await _partAlternateService.PrepareForSaveAsync(model);
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

        var approvalStateBefore = item.ApprovedAt.HasValue ? "Approved" : "PendingApproval";
        item.SourceCompanyPN = model.SourceCompanyPN;
        item.TargetCompanyPN = model.TargetCompanyPN;
        item.AltLevel = model.AltLevel;
        item.SameFootprintYN = model.SameFootprintYN;
        item.SameSymbolYN = model.SameSymbolYN;
        item.NeedEEReviewYN = model.NeedEEReviewYN;
        item.NeedLayoutReviewYN = model.NeedLayoutReviewYN;
        item.Notes = model.Notes;
        item.UpdatedBy = User.Identity?.Name;

        await WriteAuditAsync(
            item.SourceCompanyPN,
            item.TargetCompanyPN,
            ChangeType.StatusChanged,
            approvalStateBefore,
            item.ApprovedAt.HasValue ? "Approved" : "PendingApproval",
            "Alternate relation updated.");
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Alternate relation updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id)
    {
        if (!CanApproveAlternate())
        {
            return Forbid();
        }

        var item = await _dbContext.PartAlternates
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var validationResult = await _partAlternateService.ValidateApprovalAsync(item);
            if (!validationResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", validationResult.Errors);
                return RedirectToAction(nameof(Index));
            }

            item.ApprovedBy = User.Identity?.Name;
            item.ApprovedAt = DateTime.UtcNow;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                item.SourceCompanyPN,
                item.TargetCompanyPN,
                ChangeType.StatusChanged,
                "PendingApproval",
                "Approved",
                "Alternate relation approved.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Alternate relation approved.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        if (!CanEditAlternates())
        {
            return Forbid();
        }

        var item = await _dbContext.PartAlternates
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                item.SourceCompanyPN,
                item.TargetCompanyPN,
                ChangeType.AltRemoved,
                item.ApprovedAt.HasValue ? "Approved" : "PendingApproval",
                "Deleted",
                "Alternate relation soft deleted.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Alternate relation deleted.";
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

    private bool CanApproveAlternate()
        => User.IsInRole("Admin") || User.IsInRole("Librarian") || User.IsInRole("EEReviewer");

    private bool CanEditAlternates()
        => User.IsInRole("Admin") || User.IsInRole("Librarian") || User.IsInRole("EEReviewer");

    private async Task<AlternateListItemViewModel> BuildAlternateListItemAsync(PartAlternate alternate)
    {
        var descriptions = await _dbContext.CompanyParts
            .Where(x => x.CompanyPN == alternate.SourceCompanyPN || x.CompanyPN == alternate.TargetCompanyPN)
            .ToDictionaryAsync(x => x.CompanyPN, x => x.Description);

        return new AlternateListItemViewModel
        {
            Alternate = alternate,
            SourceDescription = descriptions.GetValueOrDefault(alternate.SourceCompanyPN),
            TargetDescription = descriptions.GetValueOrDefault(alternate.TargetCompanyPN)
        };
    }

    private Task WriteAuditAsync(
        string sourceCompanyPn,
        string targetCompanyPn,
        ChangeType changeType,
        string? oldValue,
        string? newValue,
        string reason)
    {
        var actor = User.Identity?.Name ?? "system";
        return _changeLogService.WriteAsync(
            $"ALTERNATE:{sourceCompanyPn}->{targetCompanyPn}",
            changeType,
            oldValue,
            newValue,
            reason,
            actor);
    }
}
