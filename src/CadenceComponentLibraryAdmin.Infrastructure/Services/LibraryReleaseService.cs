using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class LibraryReleaseService : ILibraryReleaseService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IQualityReportService _qualityReportService;

    public LibraryReleaseService(ApplicationDbContext dbContext, IQualityReportService qualityReportService)
    {
        _dbContext = dbContext;
        _qualityReportService = qualityReportService;
    }

    public async Task<LibraryReleaseDraftDto> BuildDraftAsync(CancellationToken cancellationToken = default)
    {
        return new LibraryReleaseDraftDto
        {
            ReleaseName = $"LIB_{DateTime.Now:yyyy.MM.dd}",
            ReleaseDate = DateTime.Now,
            PartCount = await _dbContext.CompanyParts.AsNoTracking().CountAsync(x => x.ApprovalStatus == ApprovalStatus.Approved, cancellationToken),
            FootprintCount = await _dbContext.FootprintVariants.AsNoTracking().CountAsync(x => x.Status == FootprintStatus.Released, cancellationToken),
            SymbolCount = await _dbContext.SymbolFamilies.AsNoTracking().CountAsync(x => x.IsActive, cancellationToken)
        };
    }

    public async Task<LibraryRelease> CreateDraftAsync(
        string releasedBy,
        string? releaseNote,
        CancellationToken cancellationToken = default)
    {
        var draft = await BuildDraftAsync(cancellationToken);
        var existingDraft = await _dbContext.LibraryReleases
            .FirstOrDefaultAsync(x => x.ReleaseName == draft.ReleaseName && x.Status == ReleaseStatus.Draft, cancellationToken);

        if (existingDraft is not null)
        {
            if (!string.IsNullOrWhiteSpace(releaseNote))
            {
                existingDraft.ReleaseNote = releaseNote;
                existingDraft.PartCount = draft.PartCount;
                existingDraft.FootprintCount = draft.FootprintCount;
                existingDraft.SymbolCount = draft.SymbolCount;
                existingDraft.UpdatedBy = releasedBy;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return existingDraft;
        }

        var item = new LibraryRelease
        {
            ReleaseName = draft.ReleaseName,
            ReleaseDate = draft.ReleaseDate,
            ReleasedBy = releasedBy,
            ReleaseNote = releaseNote,
            PartCount = draft.PartCount,
            FootprintCount = draft.FootprintCount,
            SymbolCount = draft.SymbolCount,
            Status = ReleaseStatus.Draft,
            CreatedBy = releasedBy
        };

        _dbContext.LibraryReleases.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<ReleaseAttemptResult> ReleaseAsync(
        long id,
        string releasedBy,
        CancellationToken cancellationToken = default)
    {
        var result = new ReleaseAttemptResult();
        var item = await _dbContext.LibraryReleases.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
        {
            result.Errors.Add("Library Release not found.");
            return result;
        }

        var quality = await _qualityReportService.BuildSummaryAsync(cancellationToken);
        if (quality.TotalFindings > 0)
        {
            result.Errors.Add("Quality Reports still contain open findings. Resolve them before release.");
            return result;
        }

        var duplicateName = await _dbContext.LibraryReleases
            .AsNoTracking()
            .AnyAsync(x => x.ReleaseName == item.ReleaseName && x.Id != item.Id && x.Status == ReleaseStatus.Released, cancellationToken);

        if (duplicateName)
        {
            result.Errors.Add($"Release name already exists: {item.ReleaseName}");
            return result;
        }

        var latest = await BuildDraftAsync(cancellationToken);
        item.Status = ReleaseStatus.Released;
        item.ReleasedBy = releasedBy;
        item.ReleaseDate = DateTime.Now;
        item.PartCount = latest.PartCount;
        item.FootprintCount = latest.FootprintCount;
        item.SymbolCount = latest.SymbolCount;
        item.UpdatedBy = releasedBy;
        await _dbContext.SaveChangesAsync(cancellationToken);

        result.Succeeded = true;
        return result;
    }
}
