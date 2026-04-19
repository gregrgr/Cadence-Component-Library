using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian,EEReviewer,Purchasing,Designer")]
public sealed class OnlineCandidatesController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public OnlineCandidatesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(string? search, string? sourceProvider, CandidateStatus? status, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.OnlineCandidates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Manufacturer.Contains(search) ||
                x.ManufacturerPN.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(sourceProvider))
        {
            query = query.Where(x => x.SourceProvider.Contains(sourceProvider));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.CandidateStatus == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Search = search;
        ViewBag.SourceProvider = sourceProvider;
        ViewBag.Status = status;
        return View(new PagedResult<OnlineCandidate> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount });
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    public IActionResult Create() => View(new OnlineCandidate { CandidateStatus = CandidateStatus.NewFromWeb });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OnlineCandidate model)
    {
        if (!ModelState.IsValid) return View(model);

        model.CreatedBy = User.Identity?.Name;
        _dbContext.OnlineCandidates.Add(model);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Online Candidate created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, OnlineCandidate model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.SourceProvider = model.SourceProvider;
        item.Manufacturer = model.Manufacturer;
        item.ManufacturerPN = model.ManufacturerPN;
        item.Description = model.Description;
        item.RawPackageName = model.RawPackageName;
        item.MountType = model.MountType;
        item.LeadCount = model.LeadCount;
        item.PitchMm = model.PitchMm;
        item.BodyLmm = model.BodyLmm;
        item.BodyWmm = model.BodyWmm;
        item.EPLmm = model.EPLmm;
        item.EPWmm = model.EPWmm;
        item.DatasheetUrl = model.DatasheetUrl;
        item.RoHS = model.RoHS;
        item.LifecycleStatus = model.LifecycleStatus;
        item.SymbolDownloaded = model.SymbolDownloaded;
        item.FootprintDownloaded = model.FootprintDownloaded;
        item.StepDownloaded = model.StepDownloaded;
        item.CandidateStatus = model.CandidateStatus;
        item.ImportNote = model.ImportNote;
        item.UpdatedBy = User.Identity?.Name;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Online Candidate updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Online Candidate deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
