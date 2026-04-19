using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian,EEReviewer")]
public sealed class ApprovalQueueController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICompanyPartService _companyPartService;

    public ApprovalQueueController(ApplicationDbContext dbContext, ICompanyPartService companyPartService)
    {
        _dbContext = dbContext;
        _companyPartService = companyPartService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new ApprovalQueueViewModel
        {
            PendingOnlineCandidates = await _dbContext.OnlineCandidates
                .Where(x => x.CandidateStatus == CandidateStatus.PendingReview)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(),
            PendingCompanyParts = await _dbContext.CompanyParts
                .Include(x => x.DefaultFootprint)
                .Where(x => x.ApprovalStatus == ApprovalStatus.PendingReview)
                .OrderBy(x => x.CompanyPN)
                .ToListAsync(),
            ReviewingFootprints = await _dbContext.FootprintVariants
                .Where(x => x.Status == FootprintStatus.Reviewing)
                .OrderBy(x => x.FootprintName)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOnlineCandidate(long id)
    {
        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.CandidateStatus = CandidateStatus.Approved;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Online Candidate approved.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOnlineCandidate(long id)
    {
        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.CandidateStatus = CandidateStatus.Rejected;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Online Candidate rejected.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnOnlineCandidate(long id)
    {
        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.CandidateStatus = CandidateStatus.PendingSupplyCheck;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Online Candidate returned for update.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveCompanyPart(long id)
    {
        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return RedirectToAction(nameof(Index));
        }

        item.ApprovalStatus = ApprovalStatus.Approved;
        var result = await _companyPartService.ValidateApprovalAsync(item);
        if (!result.Succeeded)
        {
            item.ApprovalStatus = ApprovalStatus.PendingReview;
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction(nameof(Index));
        }

        item.UpdatedBy = User.Identity?.Name;
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Company Part approved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectCompanyPart(long id)
    {
        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.ApprovalStatus = ApprovalStatus.Rejected;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Company Part rejected.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnCompanyPart(long id)
    {
        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.ApprovalStatus = ApprovalStatus.New;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Company Part returned for revision.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveFootprint(long id)
    {
        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.Status = FootprintStatus.Released;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Footprint Variant released.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectFootprint(long id)
    {
        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.Status = FootprintStatus.Blocked;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Footprint Variant blocked.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnFootprint(long id)
    {
        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            item.Status = FootprintStatus.Draft;
            item.UpdatedBy = User.Identity?.Name;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Footprint Variant returned to draft.";
        }

        return RedirectToAction(nameof(Index));
    }
}
