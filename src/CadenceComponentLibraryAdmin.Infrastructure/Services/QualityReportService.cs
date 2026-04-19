using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class QualityReportService : IQualityReportService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileCheckService _fileCheckService;

    public QualityReportService(ApplicationDbContext dbContext, IFileCheckService fileCheckService)
    {
        _dbContext = dbContext;
        _fileCheckService = fileCheckService;
    }

    public async Task<QualityReportSummary> BuildSummaryAsync(CancellationToken cancellationToken = default)
    {
        var sections = new List<QualityReportSection>
        {
            await BuildDuplicateMpnSectionAsync(cancellationToken),
            await BuildApprovedPartMissingMpnSectionAsync(cancellationToken),
            await BuildApprovedPartMissingFootprintSectionAsync(cancellationToken),
            await BuildApprovedPartNonReleasedFootprintSectionAsync(cancellationToken),
            await BuildMissingDatasheetSectionAsync(cancellationToken),
            await BuildDuplicatePackageSignatureSectionAsync(cancellationToken),
            await BuildOrphanFootprintSectionAsync(cancellationToken),
            await BuildMissingFilesSectionAsync()
        };

        return new QualityReportSummary
        {
            Sections = sections
        };
    }

    private async Task<QualityReportSection> BuildDuplicateMpnSectionAsync(CancellationToken cancellationToken)
    {
        var duplicates = await _dbContext.ManufacturerParts
            .AsNoTracking()
            .GroupBy(x => new { x.Manufacturer, x.ManufacturerPN })
            .Where(g => g.Count() > 1)
            .Select(g => new QualityReportItem
            {
                PrimaryKey = $"{g.Key.Manufacturer}|{g.Key.ManufacturerPN}",
                Title = $"{g.Key.Manufacturer} / {g.Key.ManufacturerPN}",
                Detail = $"Duplicate count: {g.Count()}"
            })
            .ToListAsync(cancellationToken);

        return new QualityReportSection
        {
            Code = "DuplicateMPN",
            Title = "Duplicate MPN",
            Items = duplicates
        };
    }

    private async Task<QualityReportSection> BuildApprovedPartMissingMpnSectionAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.CompanyParts
            .AsNoTracking()
            .Where(x => x.ApprovalStatus == ApprovalStatus.Approved)
            .Where(x => !_dbContext.ManufacturerParts.Any(mp => mp.CompanyPN == x.CompanyPN && mp.IsApproved))
            .Select(x => new QualityReportItem
            {
                PrimaryKey = x.CompanyPN,
                Title = x.CompanyPN,
                Detail = x.Description
            })
            .ToListAsync(cancellationToken);

        return new QualityReportSection
        {
            Code = "ApprovedPartMissingMPN",
            Title = "Approved Part Missing MPN",
            Items = items
        };
    }

    private async Task<QualityReportSection> BuildApprovedPartMissingFootprintSectionAsync(CancellationToken cancellationToken)
    {
        var footprints = _dbContext.FootprintVariants.AsNoTracking();

        var items = await _dbContext.CompanyParts
            .AsNoTracking()
            .Where(x => x.ApprovalStatus == ApprovalStatus.Approved)
            .Where(x => string.IsNullOrWhiteSpace(x.DefaultFootprintName) || !footprints.Any(fp => fp.FootprintName == x.DefaultFootprintName))
            .Select(x => new QualityReportItem
            {
                PrimaryKey = x.CompanyPN,
                Title = x.CompanyPN,
                Detail = x.DefaultFootprintName
            })
            .ToListAsync(cancellationToken);

        return new QualityReportSection
        {
            Code = "ApprovedPartMissingFootprint",
            Title = "Approved Part Missing Footprint",
            Items = items
        };
    }

    private async Task<QualityReportSection> BuildApprovedPartNonReleasedFootprintSectionAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.CompanyParts
            .AsNoTracking()
            .Join(
                _dbContext.FootprintVariants.AsNoTracking(),
                cp => cp.DefaultFootprintName,
                fp => fp.FootprintName,
                (cp, fp) => new { cp, fp })
            .Where(x => x.cp.ApprovalStatus == ApprovalStatus.Approved && x.fp.Status != FootprintStatus.Released)
            .Select(x => new QualityReportItem
            {
                PrimaryKey = x.cp.CompanyPN,
                Title = x.cp.CompanyPN,
                Detail = $"{x.cp.DefaultFootprintName} ({x.fp.Status})"
            })
            .ToListAsync(cancellationToken);

        return new QualityReportSection
        {
            Code = "ApprovedPartReferencesNonReleasedFootprint",
            Title = "Approved Part References Non-Released Footprint",
            Items = items
        };
    }

    private async Task<QualityReportSection> BuildMissingDatasheetSectionAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.CompanyParts
            .AsNoTracking()
            .Where(x => x.ApprovalStatus == ApprovalStatus.Approved && string.IsNullOrWhiteSpace(x.DatasheetUrl))
            .Select(x => new QualityReportItem
            {
                PrimaryKey = x.CompanyPN,
                Title = x.CompanyPN,
                Detail = x.Description
            })
            .ToListAsync(cancellationToken);

        return new QualityReportSection
        {
            Code = "MissingDatasheet",
            Title = "Missing Datasheet",
            Items = items
        };
    }

    private async Task<QualityReportSection> BuildDuplicatePackageSignatureSectionAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.PackageFamilies
            .AsNoTracking()
            .GroupBy(x => x.PackageSignature)
            .Where(g => g.Count() > 1)
            .Select(g => new QualityReportItem
            {
                PrimaryKey = g.Key,
                Title = g.Key,
                Detail = $"Package families: {string.Join(", ", g.Select(x => x.PackageFamilyCode))}"
            })
            .ToListAsync(cancellationToken);

        return new QualityReportSection
        {
            Code = "DuplicatePackageSignature",
            Title = "Duplicate Package Signature",
            Items = items
        };
    }

    private async Task<QualityReportSection> BuildOrphanFootprintSectionAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.FootprintVariants
            .AsNoTracking()
            .Where(x => !_dbContext.CompanyParts.Any(cp => cp.DefaultFootprintName == x.FootprintName))
            .Select(x => new QualityReportItem
            {
                PrimaryKey = x.FootprintName,
                Title = x.FootprintName,
                Detail = x.PackageFamilyCode
            })
            .ToListAsync(cancellationToken);

        return new QualityReportSection
        {
            Code = "OrphanFootprint",
            Title = "Orphan Footprint",
            Items = items
        };
    }

    private async Task<QualityReportSection> BuildMissingFilesSectionAsync()
    {
        var summary = await _fileCheckService.CheckReleasePartsAsync();

        return new QualityReportSection
        {
            Code = "MissingFiles",
            Title = "Missing Files",
            Items = summary.Issues
                .Select(x => new QualityReportItem
                {
                    PrimaryKey = $"{x.FileType}:{x.OwnerKey}",
                    Title = $"{x.OwnerType} / {x.OwnerKey}",
                    Detail = $"{x.FileType}: {x.Path ?? "(empty)"}"
                })
                .ToList()
        };
    }
}
