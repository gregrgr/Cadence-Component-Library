using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.CadenceBridge.Queue;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class McpLibraryWorkflowServiceTests
{
    [Fact]
    public async Task CannotEnqueueCaptureOrAllegroJob_UnlessExtractionIsApprovedForBuild()
    {
        await using var dbContext = CreateDbContext();
        var candidate = await CreateCandidateAsync(dbContext, "Texas Instruments", "SN74LVC1G14DBVR");
        var extraction = await CreateExtractionAsync(dbContext, candidate.Id, AiDatasheetExtractionStatus.NeedsReview);
        var service = CreateService(dbContext);

        var captureError = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EnqueueCaptureSymbolJobAsync(new CadenceEnqueueJobRequest(extraction.Id)));

        var allegroError = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EnqueueAllegroFootprintJobAsync(new CadenceEnqueueJobRequest(extraction.Id)));

        Assert.Contains("ApprovedForBuild", captureError.Message, StringComparison.Ordinal);
        Assert.Contains("ApprovedForBuild", allegroError.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EnqueuedJobs_WritePendingStatusAndQueueFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "cadence-workflow-tests", Guid.NewGuid().ToString("N"));

        try
        {
            await using var dbContext = CreateDbContext();
            var candidate = await CreateCandidateAsync(dbContext, "Texas Instruments", "SN74LVC1G14DBVR");
            var extraction = await CreateExtractionAsync(dbContext, candidate.Id, AiDatasheetExtractionStatus.ApprovedForBuild);
            var options = CreateOptions(root);
            var queue = new FileSystemCadenceJobQueue(dbContext, Options.Create(options));
            var service = CreateService(dbContext, options, queue);

            var capture = await service.EnqueueCaptureSymbolJobAsync(new CadenceEnqueueJobRequest(extraction.Id));
            var allegro = await service.EnqueueAllegroFootprintJobAsync(new CadenceEnqueueJobRequest(extraction.Id));

            Assert.Equal(CadenceBuildJobStatus.Pending, capture.Status);
            Assert.Equal(CadenceBuildJobType.CaptureSymbol, capture.JobType);
            Assert.Equal(CadenceBuildJobStatus.Pending, allegro.Status);
            Assert.Equal(CadenceBuildJobType.AllegroFootprint, allegro.JobType);
            Assert.Equal(2, await dbContext.CadenceBuildJobs.CountAsync());

            var captureFile = Path.Combine(root, "capture", "pending", $"{capture.JobId}.job.json");
            var allegroFile = Path.Combine(root, "allegro", "pending", $"{allegro.JobId}.job.json");
            Assert.True(File.Exists(captureFile));
            Assert.True(File.Exists(allegroFile));

            using var captureDocument = JsonDocument.Parse(await File.ReadAllTextAsync(captureFile));
            using var allegroDocument = JsonDocument.Parse(await File.ReadAllTextAsync(allegroFile));
            Assert.Equal("create_symbol", captureDocument.RootElement.GetProperty("action").GetString());
            Assert.Equal("fail_if_exists", captureDocument.RootElement.GetProperty("overwritePolicy").GetString());
            Assert.Equal("create_footprint", allegroDocument.RootElement.GetProperty("action").GetString());
            Assert.Equal("fail_if_exists", allegroDocument.RootElement.GetProperty("overwritePolicy").GetString());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DuplicateSearch_ReturnsExistingRecords()
    {
        await using var dbContext = CreateDbContext();

        dbContext.PackageFamilies.Add(new PackageFamily
        {
            PackageFamilyCode = "SOT23_5",
            MountType = "SMT",
            LeadCount = 5,
            PackageSignature = "SOT23-5"
        });

        dbContext.FootprintVariants.Add(new FootprintVariant
        {
            FootprintName = "SOT-23-5_TI",
            PackageFamilyCode = "SOT23_5",
            PsmPath = "draft/SOT-23-5_TI.psm",
            VariantType = "Nominal",
            Status = FootprintStatus.Draft
        });

        dbContext.CompanyParts.Add(new CompanyPart
        {
            CompanyPN = "CMP-001",
            PartClass = "Logic",
            Description = "Single inverter",
            SymbolFamilyCode = "LOGIC_INV",
            PackageFamilyCode = "SOT23_5",
            DefaultFootprintName = "SOT-23-5_TI",
            ApprovalStatus = ApprovalStatus.PendingReview,
            LifecycleStatus = LifecycleStatus.Unknown
        });

        dbContext.ManufacturerParts.Add(new ManufacturerPart
        {
            CompanyPN = "CMP-001",
            Manufacturer = "Texas Instruments",
            ManufacturerPN = "SN74LVC1G14DBVR",
            LifecycleStatus = LifecycleStatus.Unknown
        });

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var result = await service.SearchDuplicateAsync(
            new LibrarySearchDuplicateRequest(
                "Texas Instruments",
                "SN74LVC1G14DBVR",
                "SOT-23-5"));

        Assert.Single(result.ManufacturerParts);
        Assert.Single(result.CompanyParts);
        Assert.Single(result.PackageFamilies);
        Assert.Single(result.FootprintVariants);
    }

    [Fact]
    public async Task ServiceMethods_AreTestableWithoutLaunchingRealMcpClient()
    {
        await using var dbContext = CreateDbContext();
        var candidate = await CreateCandidateAsync(dbContext, "Analog Devices", "ADP150AUJZ");
        var extraction = await CreateExtractionAsync(dbContext, candidate.Id, AiDatasheetExtractionStatus.Draft);
        var service = CreateService(dbContext);

        var candidateSummary = await service.GetCandidateAsync(new LibraryGetCandidateRequest(candidate.Id, null));
        var draft = await service.SubmitForReviewAsync(extraction.Id);
        var approved = await service.ApproveForBuildAsync(extraction.Id);

        Assert.Equal(candidate.Id, candidateSummary.CandidateId);
        Assert.Equal(AiDatasheetExtractionStatus.NeedsReview, draft.Status);
        Assert.Equal(AiDatasheetExtractionStatus.ApprovedForBuild, approved.Status);
    }

    private static McpLibraryWorkflowService CreateService(
        ApplicationDbContext dbContext,
        CadenceAutomationOptions? options = null,
        ICadenceBuildJobQueue? jobQueue = null)
    {
        return new McpLibraryWorkflowService(
            dbContext,
            Options.Create(options ?? CreateOptions("storage/cadence-jobs")),
            jobQueue);
    }

    private static CadenceAutomationOptions CreateOptions(string root)
    {
        return new CadenceAutomationOptions
        {
            JobRoot = Path.Combine(root, "jobs"),
            CaptureQueuePath = Path.Combine(root, "capture"),
            AllegroQueuePath = Path.Combine(root, "allegro"),
            LibraryRoot = Path.Combine(root, "library")
        };
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<OnlineCandidate> CreateCandidateAsync(
        ApplicationDbContext dbContext,
        string manufacturer,
        string manufacturerPn)
    {
        var candidate = new OnlineCandidate
        {
            SourceProvider = "test",
            Manufacturer = manufacturer,
            ManufacturerPN = manufacturerPn,
            CandidateStatus = CandidateStatus.NewFromWeb,
            LifecycleStatus = LifecycleStatus.Unknown
        };

        dbContext.OnlineCandidates.Add(candidate);
        await dbContext.SaveChangesAsync();
        return candidate;
    }

    private static async Task<AiDatasheetExtraction> CreateExtractionAsync(
        ApplicationDbContext dbContext,
        long candidateId,
        AiDatasheetExtractionStatus status)
    {
        var extraction = new AiDatasheetExtraction
        {
            CandidateId = candidateId,
            Manufacturer = "Test Manufacturer",
            ManufacturerPartNumber = "TEST-123",
            ExtractionJson = "{\"summary\":\"test\"}",
            SymbolSpecJson = "{\"pins\":[]}",
            FootprintSpecJson = "{\"pads\":[]}",
            Confidence = 0.9m,
            Status = status
        };

        dbContext.AiDatasheetExtractions.Add(extraction);
        await dbContext.SaveChangesAsync();
        return extraction;
    }
}
