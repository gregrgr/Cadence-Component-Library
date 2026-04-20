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
    private readonly IChangeLogService _changeLogService;
    private readonly ICompanyPartService _companyPartService;

    public ApprovalQueueController(
        ApplicationDbContext dbContext,
        ICompanyPartService companyPartService,
        IChangeLogService changeLogService)
    {
        _dbContext = dbContext;
        _companyPartService = companyPartService;
        _changeLogService = changeLogService;
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
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(),
            ReviewingFootprints = await _dbContext.FootprintVariants
                .Where(x => x.Status == FootprintStatus.Reviewing)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOnlineCandidate(long id)
    {
        if (!CanApproveOnlineCandidate())
        {
            return Forbid();
        }

        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.CandidateStatus;
            item.CandidateStatus = CandidateStatus.Approved;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                $"ONLINECANDIDATE:{item.Id}",
                oldStatus.ToString(),
                item.CandidateStatus.ToString(),
                "Approved from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Online Candidate approved.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOnlineCandidate(long id)
    {
        if (!CanApproveOnlineCandidate())
        {
            return Forbid();
        }

        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.CandidateStatus;
            item.CandidateStatus = CandidateStatus.Rejected;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                $"ONLINECANDIDATE:{item.Id}",
                oldStatus.ToString(),
                item.CandidateStatus.ToString(),
                "Rejected from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Online Candidate rejected.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnOnlineCandidate(long id)
    {
        if (!CanApproveOnlineCandidate())
        {
            return Forbid();
        }

        var item = await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.CandidateStatus;
            item.CandidateStatus = CandidateStatus.PendingSupplyCheck;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                $"ONLINECANDIDATE:{item.Id}",
                oldStatus.ToString(),
                item.CandidateStatus.ToString(),
                "Returned for changes from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Online Candidate returned for update.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveCompanyPart(long id)
    {
        if (!CanApproveCompanyPart())
        {
            return Forbid();
        }

        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var oldStatus = item.ApprovalStatus;
        item.ApprovalStatus = ApprovalStatus.Approved;
        var result = await _companyPartService.ValidateApprovalAsync(item);
        if (!result.Succeeded)
        {
            item.ApprovalStatus = oldStatus;
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction(nameof(Index));
        }

        item.UpdatedBy = User.Identity?.Name;
        await WriteAuditAsync(
            item.CompanyPN,
            oldStatus.ToString(),
            item.ApprovalStatus.ToString(),
            "Approved from approval queue.");
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Company Part approved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectCompanyPart(long id)
    {
        if (!CanApproveCompanyPart())
        {
            return Forbid();
        }

        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.ApprovalStatus;
            item.ApprovalStatus = ApprovalStatus.Rejected;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                item.CompanyPN,
                oldStatus.ToString(),
                item.ApprovalStatus.ToString(),
                "Rejected from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Company Part rejected.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnCompanyPart(long id)
    {
        if (!CanApproveCompanyPart())
        {
            return Forbid();
        }

        var item = await _dbContext.CompanyParts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.ApprovalStatus;
            item.ApprovalStatus = ApprovalStatus.New;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                item.CompanyPN,
                oldStatus.ToString(),
                item.ApprovalStatus.ToString(),
                "Returned for changes from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Company Part returned for revision.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveFootprint(long id)
    {
        if (!CanApproveFootprint())
        {
            return Forbid();
        }

        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.Status;
            item.Status = FootprintStatus.Released;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                $"FOOTPRINT:{item.FootprintName}",
                oldStatus.ToString(),
                item.Status.ToString(),
                "Released from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Footprint Variant released.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectFootprint(long id)
    {
        if (!CanApproveFootprint())
        {
            return Forbid();
        }

        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.Status;
            item.Status = FootprintStatus.Blocked;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                $"FOOTPRINT:{item.FootprintName}",
                oldStatus.ToString(),
                item.Status.ToString(),
                "Rejected from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Footprint Variant blocked.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnFootprint(long id)
    {
        if (!CanApproveFootprint())
        {
            return Forbid();
        }

        var item = await _dbContext.FootprintVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            var oldStatus = item.Status;
            item.Status = FootprintStatus.Draft;
            item.UpdatedBy = User.Identity?.Name;
            await WriteAuditAsync(
                $"FOOTPRINT:{item.FootprintName}",
                oldStatus.ToString(),
                item.Status.ToString(),
                "Returned for changes from approval queue.");
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Footprint Variant returned to draft.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CanApproveCompanyPart()
        => User.IsInRole("Admin") || User.IsInRole("Librarian") || User.IsInRole("EEReviewer");

    private bool CanApproveOnlineCandidate()
        => User.IsInRole("Admin") || User.IsInRole("Librarian");

    private bool CanApproveFootprint()
        => User.IsInRole("Admin") || User.IsInRole("Librarian");

    private Task WriteAuditAsync(string identifier, string oldValue, string newValue, string reason)
    {
        var actor = User.Identity?.Name ?? "system";
        return _changeLogService.WriteAsync(
            identifier,
            ChangeType.StatusChanged,
            oldValue,
            newValue,
            reason,
            actor);
    }
}
