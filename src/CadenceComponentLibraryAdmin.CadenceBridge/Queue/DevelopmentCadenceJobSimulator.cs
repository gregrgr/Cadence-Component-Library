using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.CadenceBridge.Queue;

public sealed class DevelopmentCadenceJobSimulator : ICadenceJobSimulator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ICadenceJobQueue _jobQueue;
    private readonly CadenceAutomationOptions _options;

    public DevelopmentCadenceJobSimulator(
        ICadenceJobQueue jobQueue,
        IOptions<CadenceAutomationOptions> options)
    {
        _jobQueue = jobQueue;
        _options = options.Value;
    }

    public async Task<CadenceJobStatusResult> SimulateSuccessAsync(
        long jobId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobQueue.GetJobAsync(jobId, cancellationToken);
        if (job.Status != CadenceBuildJobStatus.Pending)
        {
            throw new InvalidOperationException("Only Pending Cadence build jobs can be simulated.");
        }

        await _jobQueue.MarkRunningAsync(jobId, cancellationToken);

        var reportPath = await WriteSimulationReportAsync(jobId, job.JobType, actor, cancellationToken);
        var outputJson = JsonSerializer.Serialize(new
        {
            jobId,
            status = "Succeeded",
            simulated = true,
            jobType = job.JobType.ToString(),
            actor,
            completedAtUtc = DateTime.UtcNow
        }, JsonOptions);

        await _jobQueue.MarkSucceededAsync(
            jobId,
            outputJson,
            [new CadenceQueueArtifactInput(CadenceBuildArtifactType.Report, reportPath)],
            cancellationToken);

        var updated = await _jobQueue.GetJobAsync(jobId, cancellationToken);
        return new CadenceJobStatusResult(
            updated.Id,
            updated.JobType,
            updated.Status,
            updated.ToolName,
            updated.ToolVersion,
            updated.CreatedAtUtc,
            updated.StartedAtUtc,
            updated.FinishedAtUtc,
            updated.ErrorMessage,
            updated.Artifacts
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new CadenceArtifactSummary(x.Id, x.ArtifactType, x.FilePath, x.Sha256, x.CreatedAtUtc))
                .ToList());
    }

    private async Task<string> WriteSimulationReportAsync(
        long jobId,
        CadenceBuildJobType jobType,
        string actor,
        CancellationToken cancellationToken)
    {
        var artifactRoot = ResolvePath(Path.Combine(_options.LibraryRoot, "_simulated-workers"));
        Directory.CreateDirectory(artifactRoot);

        var reportPath = Path.Combine(artifactRoot, $"{jobId}.{jobType}.simulation-report.json");
        var reportJson = JsonSerializer.Serialize(new
        {
            jobId,
            jobType = jobType.ToString(),
            simulated = true,
            actor,
            note = "Development-only simulation report. No real Cadence Capture or Allegro tool was executed.",
            createdAtUtc = DateTime.UtcNow
        }, JsonOptions);

        await File.WriteAllTextAsync(reportPath, reportJson, cancellationToken);
        return reportPath;
    }

    private static string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
    }
}
