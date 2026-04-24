using System.Text.Json;
using CadenceComponentLibraryAdmin.CadenceBridge.Queue;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class FileSystemCadenceJobQueueTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "cadence-queue-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Enqueue_WritesCorrectJobJson()
    {
        await using var dbContext = CreateDbContext();
        var queue = CreateQueue(dbContext);
        var job = CreateJob(dbContext, CadenceBuildJobType.CaptureSymbol, """
        {"action":"create_symbol","overwritePolicy":"fail_if_exists","specJson":"{\"pins\":[]}","jobId":1}
        """);

        await queue.EnqueueAsync(job);

        var filePath = Path.Combine(_root, "capture", "pending", $"{job.Id}.job.json");
        Assert.True(File.Exists(filePath));

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
        Assert.Equal("create_symbol", document.RootElement.GetProperty("action").GetString());
        Assert.Equal("fail_if_exists", document.RootElement.GetProperty("overwritePolicy").GetString());
    }

    [Fact]
    public async Task UnknownAction_IsRejected()
    {
        await using var dbContext = CreateDbContext();
        var queue = CreateQueue(dbContext);
        var job = CreateJob(dbContext, CadenceBuildJobType.CaptureSymbol, """
        {"action":"drop_all_symbols","overwritePolicy":"fail_if_exists"}
        """);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => queue.EnqueueAsync(job));
        Assert.Contains("Unknown or unsupported action", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FailIfExistsPolicy_IsTheDefault()
    {
        var options = CreateOptions();
        var extraction = CreateExtraction();
        var captureBuilder = new CaptureSymbolJobBuilder();
        var allegroBuilder = new AllegroFootprintJobBuilder();

        var captureJson = captureBuilder.Build(
            new CadenceBuildJob { Id = 1, ToolName = "CaptureQueue", CreatedAtUtc = DateTime.UtcNow },
            extraction,
            options);

        var allegroJson = allegroBuilder.Build(
            new CadenceBuildJob { Id = 2, ToolName = "AllegroQueue", CreatedAtUtc = DateTime.UtcNow },
            extraction,
            options);

        using var captureDoc = JsonDocument.Parse(captureJson);
        using var allegroDoc = JsonDocument.Parse(allegroJson);

        Assert.Equal("fail_if_exists", captureDoc.RootElement.GetProperty("overwritePolicy").GetString());
        Assert.Equal("fail_if_exists", allegroDoc.RootElement.GetProperty("overwritePolicy").GetString());
    }

    [Fact]
    public async Task ArtifactHashing_WorksForSampleFiles()
    {
        await using var dbContext = CreateDbContext();
        var queue = CreateQueue(dbContext);
        var job = CreateJob(dbContext, CadenceBuildJobType.AllegroFootprint, """
        {"action":"create_footprint","overwritePolicy":"fail_if_exists"}
        """);
        await queue.EnqueueAsync(job);
        await queue.MarkRunningAsync(job.Id);

        var artifactPath = Path.Combine(_root, "sample.psm");
        await File.WriteAllTextAsync(artifactPath, "sample artifact");

        await queue.MarkSucceededAsync(
            job.Id,
            """{"status":"Succeeded"}""",
            [new CadenceQueueArtifactInput(CadenceBuildArtifactType.PSM, artifactPath)]);

        var storedJob = await dbContext.CadenceBuildJobs.Include(x => x.Artifacts).SingleAsync();
        var artifact = Assert.Single(storedJob.Artifacts);
        Assert.False(string.IsNullOrWhiteSpace(artifact.Sha256));
        Assert.Equal(CadenceBuildJobStatus.Succeeded, storedJob.Status);
    }

    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private FileSystemCadenceJobQueue CreateQueue(ApplicationDbContext dbContext)
    {
        Directory.CreateDirectory(_root);
        return new FileSystemCadenceJobQueue(
            dbContext,
            Options.Create(new CadenceAutomationOptions
            {
                JobRoot = Path.Combine(_root, "jobs"),
                CaptureQueuePath = Path.Combine(_root, "capture"),
                AllegroQueuePath = Path.Combine(_root, "allegro"),
                LibraryRoot = Path.Combine(_root, "library")
            }));
    }

    private CadenceAutomationOptions CreateOptions()
    {
        return new CadenceAutomationOptions
        {
            JobRoot = Path.Combine(_root, "jobs"),
            CaptureQueuePath = Path.Combine(_root, "capture"),
            AllegroQueuePath = Path.Combine(_root, "allegro"),
            LibraryRoot = Path.Combine(_root, "library")
        };
    }

    private static AiDatasheetExtraction CreateExtraction()
    {
        return new AiDatasheetExtraction
        {
            Manufacturer = "Test Manufacturer",
            ManufacturerPartNumber = "TEST-123",
            ExtractionJson = "{}",
            SymbolSpecJson = "{\"pins\":[]}",
            FootprintSpecJson = "{\"pads\":[]}"
        };
    }

    private static CadenceBuildJob CreateJob(ApplicationDbContext dbContext, CadenceBuildJobType type, string inputJson)
    {
        var job = new CadenceBuildJob
        {
            JobType = type,
            InputJson = inputJson,
            ToolName = type == CadenceBuildJobType.CaptureSymbol ? "CaptureQueue" : "AllegroQueue",
            Status = CadenceBuildJobStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.CadenceBuildJobs.Add(job);
        dbContext.SaveChanges();
        return job;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
