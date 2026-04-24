using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class AiCadencePipelineModelTests
{
    [Fact]
    public void NewEntities_HaveExpectedDefaultStatuses()
    {
        var extraction = new AiDatasheetExtraction();
        var evidence = new AiExtractionEvidence();
        var job = new CadenceBuildJob();

        Assert.Equal(AiDatasheetExtractionStatus.Draft, extraction.Status);
        Assert.Equal(AiExtractionReviewerDecision.Pending, evidence.ReviewerDecision);
        Assert.Equal(CadenceBuildJobStatus.Pending, job.Status);
    }

    [Fact]
    public void Model_MapsRequiredFields()
    {
        using var dbContext = CreateDbContext();

        var extractionEntity = dbContext.Model.FindEntityType(typeof(AiDatasheetExtraction));
        Assert.NotNull(extractionEntity);
        Assert.False(extractionEntity!.FindProperty(nameof(AiDatasheetExtraction.Manufacturer))!.IsNullable);
        Assert.False(extractionEntity.FindProperty(nameof(AiDatasheetExtraction.ManufacturerPartNumber))!.IsNullable);
        Assert.False(extractionEntity.FindProperty(nameof(AiDatasheetExtraction.ExtractionJson))!.IsNullable);
        Assert.False(extractionEntity.FindProperty(nameof(AiDatasheetExtraction.SymbolSpecJson))!.IsNullable);
        Assert.False(extractionEntity.FindProperty(nameof(AiDatasheetExtraction.FootprintSpecJson))!.IsNullable);

        var evidenceEntity = dbContext.Model.FindEntityType(typeof(AiExtractionEvidence));
        Assert.NotNull(evidenceEntity);
        Assert.False(evidenceEntity!.FindProperty(nameof(AiExtractionEvidence.FieldPath))!.IsNullable);
        Assert.False(evidenceEntity.FindProperty(nameof(AiExtractionEvidence.ValueText))!.IsNullable);

        var jobEntity = dbContext.Model.FindEntityType(typeof(CadenceBuildJob));
        Assert.NotNull(jobEntity);
        Assert.False(jobEntity!.FindProperty(nameof(CadenceBuildJob.InputJson))!.IsNullable);
        Assert.False(jobEntity.FindProperty(nameof(CadenceBuildJob.ToolName))!.IsNullable);

        var artifactEntity = dbContext.Model.FindEntityType(typeof(CadenceBuildArtifact));
        Assert.NotNull(artifactEntity);
        Assert.False(artifactEntity!.FindProperty(nameof(CadenceBuildArtifact.FilePath))!.IsNullable);
    }

    [Fact]
    public async Task Relationships_PersistAcrossExtractionEvidenceJobAndArtifact()
    {
        await using var dbContext = CreateDbContext();

        var candidate = new OnlineCandidate
        {
            SourceProvider = "test",
            Manufacturer = "Test Manufacturer",
            ManufacturerPN = "TEST-123",
            CandidateStatus = CandidateStatus.NewFromWeb
        };

        var externalImport = new ExternalComponentImport
        {
            SourceName = "test",
            ImportKey = "test:1",
            LastImportedAt = DateTime.UtcNow,
            ImportStatus = ExternalImportStatus.Imported
        };

        dbContext.OnlineCandidates.Add(candidate);
        dbContext.ExternalComponentImports.Add(externalImport);
        await dbContext.SaveChangesAsync();

        var extraction = new AiDatasheetExtraction
        {
            CandidateId = candidate.Id,
            ExternalImportId = externalImport.Id,
            Manufacturer = "Test Manufacturer",
            ManufacturerPartNumber = "TEST-123",
            ExtractionJson = "{\"kind\":\"component\"}",
            SymbolSpecJson = "{\"symbol\":\"ok\"}",
            FootprintSpecJson = "{\"footprint\":\"ok\"}",
            Confidence = 0.91m,
            Status = AiDatasheetExtractionStatus.NeedsReview
        };

        dbContext.AiDatasheetExtractions.Add(extraction);
        await dbContext.SaveChangesAsync();

        var evidence = new AiExtractionEvidence
        {
            AiDatasheetExtractionId = extraction.Id,
            FieldPath = "pins[0].name",
            ValueText = "VCC",
            Confidence = 0.88m
        };

        var job = new CadenceBuildJob
        {
            AiDatasheetExtractionId = extraction.Id,
            CandidateId = candidate.Id,
            JobType = CadenceBuildJobType.CaptureSymbol,
            InputJson = "{\"action\":\"build-symbol\"}",
            ToolName = "FakeCaptureRunner"
        };

        dbContext.AiExtractionEvidenceItems.Add(evidence);
        dbContext.CadenceBuildJobs.Add(job);
        await dbContext.SaveChangesAsync();

        var artifact = new CadenceBuildArtifact
        {
            CadenceBuildJobId = job.Id,
            ArtifactType = CadenceBuildArtifactType.Json,
            FilePath = "storage/builds/test.json",
            Sha256 = "abc123"
        };

        var report = new LibraryVerificationReport
        {
            CandidateId = candidate.Id,
            AiDatasheetExtractionId = extraction.Id,
            OverallStatus = LibraryVerificationOverallStatus.Warning,
            SymbolReportJson = "{\"symbol\":[]}"
        };

        dbContext.CadenceBuildArtifacts.Add(artifact);
        dbContext.LibraryVerificationReports.Add(report);
        await dbContext.SaveChangesAsync();

        var storedExtraction = await dbContext.AiDatasheetExtractions
            .Include(x => x.EvidenceItems)
            .Include(x => x.BuildJobs)
            .Include(x => x.VerificationReports)
            .SingleAsync();

        var storedJob = await dbContext.CadenceBuildJobs
            .Include(x => x.Artifacts)
            .SingleAsync();

        Assert.Single(storedExtraction.EvidenceItems);
        Assert.Single(storedExtraction.BuildJobs);
        Assert.Single(storedExtraction.VerificationReports);
        Assert.Single(storedJob.Artifacts);
        Assert.Equal(candidate.Id, storedExtraction.CandidateId);
        Assert.Equal(externalImport.Id, storedExtraction.ExternalImportId);
    }

    [Fact]
    public void SchemaFiles_Exist()
    {
        var repoRoot = GetRepoRoot();
        var expectedFiles = new[]
        {
            "component_extraction.schema.json",
            "symbol_spec.schema.json",
            "footprint_spec.schema.json",
            "cadence_build_result.schema.json",
            "verification_report.schema.json"
        };

        foreach (var file in expectedFiles)
        {
            Assert.True(File.Exists(Path.Combine(repoRoot, "schemas", file)), $"Schema file '{file}' should exist.");
        }
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string GetRepoRoot()
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
