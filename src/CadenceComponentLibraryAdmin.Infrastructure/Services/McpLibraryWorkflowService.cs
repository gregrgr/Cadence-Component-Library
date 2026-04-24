using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class McpLibraryWorkflowService : IMcpLibraryWorkflowService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CadenceAutomationOptions _options;

    public McpLibraryWorkflowService(
        ApplicationDbContext dbContext,
        IOptions<CadenceAutomationOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<LibraryCandidateSummaryResult> GetCandidateAsync(
        LibraryGetCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.CandidateId.HasValue && !request.ExternalImportId.HasValue)
        {
            throw new InvalidOperationException("candidateId or externalImportId is required.");
        }

        OnlineCandidate? candidate = null;
        ExternalComponentImport? externalImport = null;

        if (request.CandidateId.HasValue)
        {
            candidate = await _dbContext.OnlineCandidates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CandidateId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Candidate '{request.CandidateId.Value}' was not found.");
        }

        if (request.ExternalImportId.HasValue)
        {
            externalImport = await _dbContext.ExternalComponentImports
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ExternalImportId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"External import '{request.ExternalImportId.Value}' was not found.");
        }

        if (candidate is null && externalImport?.CandidateId is long candidateId)
        {
            candidate = await _dbContext.OnlineCandidates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == candidateId, cancellationToken);
        }

        var extractionQuery = _dbContext.AiDatasheetExtractions.AsNoTracking().AsQueryable();
        if (request.CandidateId.HasValue)
        {
            extractionQuery = extractionQuery.Where(x => x.CandidateId == request.CandidateId.Value);
        }
        else if (request.ExternalImportId.HasValue)
        {
            extractionQuery = extractionQuery.Where(x => x.ExternalImportId == request.ExternalImportId.Value);
        }

        var extractions = await extractionQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AiExtractionStatusSummary(
                x.Id,
                x.Status,
                x.Confidence,
                x.CreatedAtUtc,
                x.ReviewedAtUtc))
            .ToListAsync(cancellationToken);

        var extractionIds = extractions.Select(x => x.ExtractionId).ToList();
        var buildJobs = await _dbContext.CadenceBuildJobs
            .AsNoTracking()
            .Where(x =>
                (candidate != null && x.CandidateId == candidate.Id) ||
                (extractionIds.Count > 0 && x.AiDatasheetExtractionId.HasValue && extractionIds.Contains(x.AiDatasheetExtractionId.Value)))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new CadenceJobStatusSummary(
                x.Id,
                x.JobType,
                x.Status,
                x.CreatedAtUtc,
                x.FinishedAtUtc))
            .ToListAsync(cancellationToken);

        return new LibraryCandidateSummaryResult(
            candidate?.Id,
            externalImport?.Id ?? request.ExternalImportId,
            candidate?.SourceProvider ?? externalImport?.SourceName,
            candidate?.Manufacturer ?? externalImport?.Manufacturer,
            candidate?.ManufacturerPN ?? externalImport?.ManufacturerPN ?? externalImport?.Name,
            candidate?.Description ?? externalImport?.Description,
            candidate?.RawPackageName ?? externalImport?.PackageName ?? externalImport?.FootprintName,
            candidate?.CandidateStatus,
            extractions,
            buildJobs);
    }

    public async Task<LibraryDuplicateSearchResult> SearchDuplicateAsync(
        LibrarySearchDuplicateRequest request,
        CancellationToken cancellationToken = default)
    {
        var manufacturer = NormalizeRequired(request.Manufacturer, nameof(request.Manufacturer));
        var manufacturerPartNumber = NormalizeRequired(request.ManufacturerPartNumber, nameof(request.ManufacturerPartNumber));
        var packageName = Normalize(request.PackageName);
        var normalizedPackageName = Canonicalize(packageName);

        var manufacturerParts = await _dbContext.ManufacturerParts
            .AsNoTracking()
            .Where(x => x.Manufacturer == manufacturer && x.ManufacturerPN == manufacturerPartNumber)
            .OrderBy(x => x.CompanyPN)
            .Select(x => new DuplicateMatchSummary(
                x.Id,
                $"{x.Manufacturer} / {x.ManufacturerPN}",
                x.CompanyPN,
                "Exact manufacturer + manufacturer part number match"))
            .ToListAsync(cancellationToken);

        var companyPnMatches = manufacturerParts
            .Select(x => x.Description)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var companyParts = await _dbContext.CompanyParts
            .AsNoTracking()
            .Where(x =>
                companyPnMatches.Contains(x.CompanyPN) ||
                (!string.IsNullOrWhiteSpace(packageName) && x.DefaultFootprintName == packageName))
            .OrderBy(x => x.CompanyPN)
            .Select(x => new DuplicateMatchSummary(
                x.Id,
                x.CompanyPN,
                x.Description,
                !string.IsNullOrWhiteSpace(packageName) && x.DefaultFootprintName == packageName
                    ? "Package/footprint match"
                    : "Referenced by matching manufacturer part"))
            .ToListAsync(cancellationToken);

        var packageFamilies = string.IsNullOrWhiteSpace(packageName)
            ? new List<DuplicateMatchSummary>()
            : (await _dbContext.PackageFamilies
                .AsNoTracking()
                .OrderBy(x => x.PackageFamilyCode)
                .Select(x => new DuplicateMatchSummary(
                    x.Id,
                    x.PackageFamilyCode,
                    x.PackageStd,
                    "Package-family candidate match"))
                .ToListAsync(cancellationToken))
                .Where(x => MatchesPackageCandidate(x.DisplayName, x.Description, normalizedPackageName))
                .ToList();

        var footprintVariants = string.IsNullOrWhiteSpace(packageName)
            ? new List<DuplicateMatchSummary>()
            : (await _dbContext.FootprintVariants
                .AsNoTracking()
                .OrderBy(x => x.FootprintName)
                .Select(x => new DuplicateMatchSummary(
                    x.Id,
                    x.FootprintName,
                    x.PackageFamilyCode,
                    "Footprint candidate match"))
                .ToListAsync(cancellationToken))
                .Where(x => MatchesPackageCandidate(x.DisplayName, x.Description, normalizedPackageName))
                .ToList();

        return new LibraryDuplicateSearchResult(companyParts, manufacturerParts, packageFamilies, footprintVariants);
    }

    public async Task<DatasheetExtractionResult> CreateExtractionDraftAsync(
        DatasheetCreateExtractionDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.CandidateId.HasValue && !request.ExternalImportId.HasValue)
        {
            throw new InvalidOperationException("candidateId or externalImportId is required.");
        }

        var candidate = request.CandidateId.HasValue
            ? await _dbContext.OnlineCandidates.FirstOrDefaultAsync(x => x.Id == request.CandidateId.Value, cancellationToken)
            : null;
        var externalImport = request.ExternalImportId.HasValue
            ? await _dbContext.ExternalComponentImports.FirstOrDefaultAsync(x => x.Id == request.ExternalImportId.Value, cancellationToken)
            : null;

        if (request.CandidateId.HasValue && candidate is null)
        {
            throw new InvalidOperationException($"Candidate '{request.CandidateId.Value}' was not found.");
        }

        if (request.ExternalImportId.HasValue && externalImport is null)
        {
            throw new InvalidOperationException($"External import '{request.ExternalImportId.Value}' was not found.");
        }

        var manufacturer = candidate?.Manufacturer ?? externalImport?.Manufacturer
            ?? throw new InvalidOperationException("A manufacturer is required to create an extraction draft.");
        var mpn = candidate?.ManufacturerPN ?? externalImport?.ManufacturerPN ?? externalImport?.Name
            ?? throw new InvalidOperationException("A manufacturer part number is required to create an extraction draft.");

        var entity = new AiDatasheetExtraction
        {
            CandidateId = candidate?.Id,
            ExternalImportId = externalImport?.Id,
            Manufacturer = manufacturer,
            ManufacturerPartNumber = mpn,
            DatasheetAssetPath = Normalize(request.DatasheetAssetPath),
            ExtractionJson = NormalizeRequired(request.ExtractionJson, nameof(request.ExtractionJson)),
            SymbolSpecJson = NormalizeRequired(request.SymbolSpecJson, nameof(request.SymbolSpecJson)),
            FootprintSpecJson = NormalizeRequired(request.FootprintSpecJson, nameof(request.FootprintSpecJson)),
            Confidence = 0m,
            Status = AiDatasheetExtractionStatus.Draft
        };

        _dbContext.AiDatasheetExtractions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DatasheetExtractionResult(entity.Id, entity.Status);
    }

    public async Task<DatasheetExtractionResult> SubmitForReviewAsync(
        long extractionId,
        CancellationToken cancellationToken = default)
    {
        var extraction = await GetExtractionAsync(extractionId, cancellationToken);
        extraction.Status = AiDatasheetExtractionStatus.NeedsReview;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new DatasheetExtractionResult(extraction.Id, extraction.Status);
    }

    public async Task<DatasheetExtractionResult> ApproveForBuildAsync(
        long extractionId,
        CancellationToken cancellationToken = default)
    {
        var extraction = await GetExtractionAsync(extractionId, cancellationToken);
        extraction.Status = AiDatasheetExtractionStatus.ApprovedForBuild;
        extraction.ReviewedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new DatasheetExtractionResult(extraction.Id, extraction.Status);
    }

    public Task<CadenceJobStatusResult> EnqueueCaptureSymbolJobAsync(
        CadenceEnqueueJobRequest request,
        CancellationToken cancellationToken = default)
        => EnqueueJobAsync(request.ExtractionId, CadenceBuildJobType.CaptureSymbol, "CaptureQueue", _options.CaptureQueuePath, cancellationToken);

    public Task<CadenceJobStatusResult> EnqueueAllegroFootprintJobAsync(
        CadenceEnqueueJobRequest request,
        CancellationToken cancellationToken = default)
        => EnqueueJobAsync(request.ExtractionId, CadenceBuildJobType.AllegroFootprint, "AllegroQueue", _options.AllegroQueuePath, cancellationToken);

    public async Task<CadenceJobStatusResult> GetJobStatusAsync(
        long jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.CadenceBuildJobs
            .AsNoTracking()
            .Include(x => x.Artifacts)
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Cadence build job '{jobId}' was not found.");

        return ToJobStatusResult(job);
    }

    public async Task<VerificationReportResult?> GetVerificationReportAsync(
        VerificationGetReportRequest request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<LibraryVerificationReport> query = _dbContext.LibraryVerificationReports.AsNoTracking();

        if (request.ExtractionId.HasValue)
        {
            query = query.Where(x => x.AiDatasheetExtractionId == request.ExtractionId.Value);
        }
        else if (request.JobId.HasValue)
        {
            var job = await _dbContext.CadenceBuildJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.JobId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Cadence build job '{request.JobId.Value}' was not found.");

            if (job.AiDatasheetExtractionId.HasValue)
            {
                query = query.Where(x => x.AiDatasheetExtractionId == job.AiDatasheetExtractionId.Value);
            }
            else if (job.CandidateId.HasValue)
            {
                query = query.Where(x => x.CandidateId == job.CandidateId.Value);
            }
            else
            {
                return null;
            }
        }
        else
        {
            throw new InvalidOperationException("extractionId or jobId is required.");
        }

        var report = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return report is null
            ? null
            : new VerificationReportResult(
                report.Id,
                report.CandidateId,
                report.CompanyPartId,
                report.AiDatasheetExtractionId,
                report.OverallStatus,
                report.CreatedAtUtc,
                report.SymbolReportJson,
                report.FootprintReportJson);
    }

    private async Task<CadenceJobStatusResult> EnqueueJobAsync(
        long extractionId,
        CadenceBuildJobType jobType,
        string toolName,
        string queuePath,
        CancellationToken cancellationToken)
    {
        var extraction = await GetExtractionAsync(extractionId, cancellationToken);
        if (extraction.Status != AiDatasheetExtractionStatus.ApprovedForBuild)
        {
            throw new InvalidOperationException("Extraction must be ApprovedForBuild before a Cadence build job can be enqueued.");
        }

        var payload = JsonSerializer.Serialize(new
        {
            extractionId,
            queuePath,
            libraryRoot = _options.LibraryRoot,
            jobType = jobType.ToString()
        });

        var job = new CadenceBuildJob
        {
            CandidateId = extraction.CandidateId,
            AiDatasheetExtractionId = extraction.Id,
            JobType = jobType,
            InputJson = payload,
            Status = CadenceBuildJobStatus.Pending,
            ToolName = toolName,
            ToolVersion = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.CadenceBuildJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetJobStatusAsync(job.Id, cancellationToken);
    }

    private async Task<AiDatasheetExtraction> GetExtractionAsync(long extractionId, CancellationToken cancellationToken)
    {
        return await _dbContext.AiDatasheetExtractions
            .FirstOrDefaultAsync(x => x.Id == extractionId, cancellationToken)
            ?? throw new InvalidOperationException($"AI extraction '{extractionId}' was not found.");
    }

    private static CadenceJobStatusResult ToJobStatusResult(CadenceBuildJob job)
    {
        return new CadenceJobStatusResult(
            job.Id,
            job.JobType,
            job.Status,
            job.ToolName,
            job.ToolVersion,
            job.CreatedAtUtc,
            job.StartedAtUtc,
            job.FinishedAtUtc,
            job.ErrorMessage,
            job.Artifacts
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new CadenceArtifactSummary(x.Id, x.ArtifactType, x.FilePath, x.Sha256, x.CreatedAtUtc))
                .ToList());
    }

    private static string NormalizeRequired(string? value, string paramName)
    {
        var normalized = Normalize(value);
        return normalized ?? throw new InvalidOperationException($"{paramName} is required.");
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool MatchesPackageCandidate(string? primary, string? secondary, string? normalizedPackageName)
    {
        if (string.IsNullOrWhiteSpace(normalizedPackageName))
        {
            return false;
        }

        var primaryValue = Canonicalize(primary);
        var secondaryValue = Canonicalize(secondary);

        return (!string.IsNullOrWhiteSpace(primaryValue) && primaryValue.Contains(normalizedPackageName, StringComparison.Ordinal)) ||
               (!string.IsNullOrWhiteSpace(secondaryValue) && secondaryValue.Contains(normalizedPackageName, StringComparison.Ordinal));
    }

    private static string? Canonicalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }
}
