using System.Security.Cryptography;
using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.Cadence;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.CadenceBridge.Queue;

public sealed class FileSystemCadenceJobQueue : ICadenceJobQueue
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly CadenceAutomationOptions _options;

    public FileSystemCadenceJobQueue(
        ApplicationDbContext dbContext,
        IOptions<CadenceAutomationOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task EnqueueAsync(CadenceBuildJob job, CancellationToken cancellationToken = default)
    {
        await EnsureJobExistsAsync(job.Id, cancellationToken);
        ValidateAllowedAction(job);

        var pendingPath = GetJobFilePath(job, "pending");
        Directory.CreateDirectory(Path.GetDirectoryName(pendingPath)!);
        await File.WriteAllTextAsync(pendingPath, PrettyJson(job.InputJson), cancellationToken);
    }

    public async Task<CadenceBuildJob> GetJobAsync(long jobId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CadenceBuildJobs
            .Include(x => x.Artifacts)
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Cadence build job '{jobId}' was not found.");
    }

    public async Task MarkRunningAsync(long jobId, CancellationToken cancellationToken = default)
    {
        var job = await GetJobAsync(jobId, cancellationToken);
        MoveFirstExistingJobFile(job, "pending", "running");
        job.Status = CadenceBuildJobStatus.Running;
        job.StartedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkSucceededAsync(
        long jobId,
        string outputJson,
        IReadOnlyCollection<CadenceQueueArtifactInput> artifacts,
        CancellationToken cancellationToken = default)
    {
        var job = await GetJobAsync(jobId, cancellationToken);
        MoveFirstExistingJobFile(job, "running", "done");

        job.Status = CadenceBuildJobStatus.Succeeded;
        job.OutputJson = PrettyJson(outputJson);
        job.ErrorMessage = null;
        job.FinishedAtUtc = DateTime.UtcNow;

        foreach (var artifact in artifacts)
        {
            var entity = new CadenceBuildArtifact
            {
                CadenceBuildJobId = job.Id,
                ArtifactType = artifact.ArtifactType,
                FilePath = artifact.FilePath,
                Sha256 = File.Exists(artifact.FilePath) ? ComputeSha256(artifact.FilePath) : null,
                CreatedAtUtc = DateTime.UtcNow
            };
            _dbContext.CadenceBuildArtifacts.Add(entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await WriteResultFileAsync(job, "done", job.OutputJson ?? "{}", cancellationToken);
    }

    public async Task MarkFailedAsync(long jobId, string error, CancellationToken cancellationToken = default)
    {
        var job = await GetJobAsync(jobId, cancellationToken);
        MoveFirstExistingJobFile(job, "running", "failed");

        job.Status = CadenceBuildJobStatus.Failed;
        job.ErrorMessage = error;
        job.FinishedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var resultJson = JsonSerializer.Serialize(new
        {
            jobId,
            status = "failed",
            error
        }, JsonOptions);

        await WriteResultFileAsync(job, "failed", resultJson, cancellationToken);
    }

    private async Task EnsureJobExistsAsync(long jobId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.CadenceBuildJobs.AnyAsync(x => x.Id == jobId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"Cadence build job '{jobId}' was not found.");
        }
    }

    private static string PrettyJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, JsonOptions);
    }

    private static void ValidateAllowedAction(CadenceBuildJob job)
    {
        using var document = JsonDocument.Parse(job.InputJson);
        var action = document.RootElement.TryGetProperty("action", out var actionElement)
            ? actionElement.GetString()
            : null;

        var expectedAction = job.JobType switch
        {
            CadenceBuildJobType.CaptureSymbol => CadenceQueueActions.CreateSymbol,
            CadenceBuildJobType.AllegroFootprint => CadenceQueueActions.CreateFootprint,
            _ => throw new InvalidOperationException($"Unsupported job type '{job.JobType}'.")
        };

        if (!string.Equals(action, expectedAction, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unknown or unsupported action '{action}'.");
        }
    }

    private string GetJobFilePath(CadenceBuildJob job, string state)
    {
        var queueRoot = GetQueueRoot(job.JobType);
        return Path.Combine(queueRoot, state, $"{job.Id}.job.json");
    }

    private string GetResultFilePath(CadenceBuildJob job, string state)
    {
        var queueRoot = GetQueueRoot(job.JobType);
        return Path.Combine(queueRoot, state, $"{job.Id}.result.json");
    }

    private string GetQueueRoot(CadenceBuildJobType jobType)
    {
        return jobType switch
        {
            CadenceBuildJobType.CaptureSymbol => ResolvePath(_options.CaptureQueuePath),
            CadenceBuildJobType.AllegroFootprint => ResolvePath(_options.AllegroQueuePath),
            _ => ResolvePath(_options.JobRoot)
        };
    }

    private string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
    }

    private void MoveFirstExistingJobFile(CadenceBuildJob job, string preferredSourceState, string targetState)
    {
        var sourceCandidates = new[]
        {
            GetJobFilePath(job, preferredSourceState),
            GetJobFilePath(job, "pending"),
            GetJobFilePath(job, "running")
        }.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var source = sourceCandidates.FirstOrDefault(File.Exists);
        var target = GetJobFilePath(job, targetState);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        if (source is null)
        {
            File.WriteAllText(target, PrettyJson(job.InputJson));
            return;
        }

        if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(target))
        {
            File.Delete(target);
        }

        File.Move(source, target);
    }

    private async Task WriteResultFileAsync(CadenceBuildJob job, string state, string json, CancellationToken cancellationToken)
    {
        var resultPath = GetResultFilePath(job, state);
        Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
        await File.WriteAllTextAsync(resultPath, PrettyJson(json), cancellationToken);
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }
}
