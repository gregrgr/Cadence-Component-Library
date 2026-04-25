using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class CadenceVerificationReportService : ICadenceVerificationReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ApplicationDbContext _dbContext;

    public CadenceVerificationReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VerificationReportResult> GenerateDevelopmentReportAsync(
        long extractionId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var extraction = await _dbContext.AiDatasheetExtractions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == extractionId, cancellationToken)
            ?? throw new InvalidOperationException($"AI extraction '{extractionId}' was not found.");

        var jobs = await _dbContext.CadenceBuildJobs
            .AsNoTracking()
            .Include(x => x.Artifacts)
            .Where(x => x.AiDatasheetExtractionId == extractionId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (jobs.Count == 0)
        {
            throw new InvalidOperationException("At least one Cadence build job is required before generating a verification report.");
        }

        var symbolJobs = jobs.Where(x => x.JobType == CadenceBuildJobType.CaptureSymbol).ToList();
        var footprintJobs = jobs.Where(x => x.JobType == CadenceBuildJobType.AllegroFootprint).ToList();
        var hasSucceededSymbol = symbolJobs.Any(x => x.Status == CadenceBuildJobStatus.Succeeded);
        var hasSucceededFootprint = footprintJobs.Any(x => x.Status == CadenceBuildJobStatus.Succeeded);

        var overallStatus = hasSucceededSymbol && hasSucceededFootprint
            ? LibraryVerificationOverallStatus.Pass
            : jobs.Any(x => x.Status == CadenceBuildJobStatus.Failed)
                ? LibraryVerificationOverallStatus.Fail
                : LibraryVerificationOverallStatus.Warning;

        var report = new LibraryVerificationReport
        {
            CandidateId = extraction.CandidateId,
            AiDatasheetExtractionId = extraction.Id,
            OverallStatus = overallStatus,
            CreatedAtUtc = DateTime.UtcNow,
            SymbolReportJson = BuildReportJson(
                extraction,
                CadenceBuildJobType.CaptureSymbol,
                symbolJobs,
                hasSucceededSymbol,
                actor),
            FootprintReportJson = BuildReportJson(
                extraction,
                CadenceBuildJobType.AllegroFootprint,
                footprintJobs,
                hasSucceededFootprint,
                actor)
        };

        _dbContext.LibraryVerificationReports.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VerificationReportResult(
            report.Id,
            report.CandidateId,
            report.CompanyPartId,
            report.AiDatasheetExtractionId,
            report.OverallStatus,
            report.CreatedAtUtc,
            report.SymbolReportJson,
            report.FootprintReportJson);
    }

    private static string BuildReportJson(
        AiDatasheetExtraction extraction,
        CadenceBuildJobType jobType,
        IReadOnlyCollection<CadenceBuildJob> jobs,
        bool hasSucceededJob,
        string actor)
    {
        var artifactSummaries = jobs
            .SelectMany(x => x.Artifacts.Select(artifact => new
            {
                jobId = x.Id,
                artifact.ArtifactType,
                artifact.FilePath,
                artifact.Sha256,
                artifact.CreatedAtUtc
            }))
            .ToList();

        return JsonSerializer.Serialize(new
        {
            simulated = true,
            extractionId = extraction.Id,
            jobType = jobType.ToString(),
            status = hasSucceededJob ? "Pass" : "Warning",
            actor,
            generatedAtUtc = DateTime.UtcNow,
            note = "Development verification report generated from queued job status and recorded artifacts. No real Cadence verification tool was executed.",
            jobs = jobs.Select(x => new
            {
                x.Id,
                x.Status,
                x.ToolName,
                x.CreatedAtUtc,
                x.StartedAtUtc,
                x.FinishedAtUtc,
                x.ErrorMessage
            }),
            artifacts = artifactSummaries
        }, JsonOptions);
    }
}
