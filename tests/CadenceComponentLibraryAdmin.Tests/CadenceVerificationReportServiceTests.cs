using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class CadenceVerificationReportServiceTests
{
    [Fact]
    public async Task DevelopmentReport_UsesSucceededJobsAndDoesNotPublishParts()
    {
        await using var dbContext = CreateDbContext();
        var extraction = new AiDatasheetExtraction
        {
            Manufacturer = "Test Manufacturer",
            ManufacturerPartNumber = "TEST-123",
            ExtractionJson = "{}",
            SymbolSpecJson = "{}",
            FootprintSpecJson = "{}",
            Confidence = 0.9m,
            Status = AiDatasheetExtractionStatus.ApprovedForBuild
        };
        dbContext.AiDatasheetExtractions.Add(extraction);
        await dbContext.SaveChangesAsync();

        AddSucceededJob(dbContext, extraction.Id, CadenceBuildJobType.CaptureSymbol, "symbol-report.json");
        AddSucceededJob(dbContext, extraction.Id, CadenceBuildJobType.AllegroFootprint, "footprint-report.json");
        await dbContext.SaveChangesAsync();

        var service = new CadenceVerificationReportService(dbContext);
        var result = await service.GenerateDevelopmentReportAsync(extraction.Id, "test-user");

        Assert.Equal(LibraryVerificationOverallStatus.Pass, result.OverallStatus);
        Assert.Contains("\"simulated\": true", result.SymbolReportJson, StringComparison.Ordinal);
        Assert.Contains("\"simulated\": true", result.FootprintReportJson, StringComparison.Ordinal);
        Assert.Equal(0, await dbContext.CompanyParts.CountAsync());
        Assert.Equal(1, await dbContext.LibraryVerificationReports.CountAsync());
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void AddSucceededJob(
        ApplicationDbContext dbContext,
        long extractionId,
        CadenceBuildJobType jobType,
        string artifactPath)
    {
        dbContext.CadenceBuildJobs.Add(new CadenceBuildJob
        {
            AiDatasheetExtractionId = extractionId,
            JobType = jobType,
            InputJson = "{}",
            OutputJson = "{\"status\":\"Succeeded\"}",
            Status = CadenceBuildJobStatus.Succeeded,
            ToolName = jobType == CadenceBuildJobType.CaptureSymbol ? "CaptureQueue" : "AllegroQueue",
            CreatedAtUtc = DateTime.UtcNow,
            FinishedAtUtc = DateTime.UtcNow,
            Artifacts =
            [
                new CadenceBuildArtifact
                {
                    ArtifactType = CadenceBuildArtifactType.Report,
                    FilePath = artifactPath,
                    Sha256 = "ABC123",
                    CreatedAtUtc = DateTime.UtcNow
                }
            ]
        });
    }
}
