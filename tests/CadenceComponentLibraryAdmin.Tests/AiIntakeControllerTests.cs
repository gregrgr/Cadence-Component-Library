using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Controllers;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class AiIntakeControllerTests
{
    [Fact]
    public async Task UnauthorizedUsers_CannotApproveForBuild()
    {
        await using var dbContext = CreateDbContext();
        var extraction = await SeedExtractionAsync(dbContext, AiDatasheetExtractionStatus.NeedsReview);
        var controller = CreateController(dbContext, "Designer");

        var result = await controller.ApproveForBuild(extraction.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task BuildButtons_DisabledUnlessApprovedForBuild()
    {
        await using var dbContext = CreateDbContext();
        var extraction = await SeedExtractionAsync(dbContext, AiDatasheetExtractionStatus.NeedsReview);
        var controller = CreateController(dbContext, "Librarian");

        var result = await controller.Details(extraction.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AiIntakeDetailsViewModel>(view.Model);
        Assert.False(model.BuildActionsEnabled);
        Assert.False(model.CanBuildSymbol);
        Assert.False(model.CanBuildFootprint);
    }

    [Fact]
    public async Task InvalidJson_CannotBeSaved()
    {
        await using var dbContext = CreateDbContext();
        var extraction = await SeedExtractionAsync(dbContext, AiDatasheetExtractionStatus.Draft);
        var controller = CreateController(dbContext, "Designer");

        var model = new AiIntakeEditViewModel
        {
            Id = extraction.Id,
            Manufacturer = extraction.Manufacturer,
            ManufacturerPartNumber = extraction.ManufacturerPartNumber,
            ExtractionJson = "{ invalid",
            SymbolSpecJson = "{}",
            FootprintSpecJson = "{}",
            Status = extraction.Status
        };

        var result = await controller.Edit(extraction.Id, model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<AiIntakeEditViewModel>(view.Model);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task JobCreation_CreatesPendingJob()
    {
        await using var dbContext = CreateDbContext();
        var extraction = await SeedExtractionAsync(dbContext, AiDatasheetExtractionStatus.ApprovedForBuild);
        var controller = CreateController(dbContext, "Librarian");

        var result = await controller.BuildSymbol(extraction.Id);

        Assert.IsType<RedirectToActionResult>(result);
        var job = await dbContext.CadenceBuildJobs.SingleAsync();
        Assert.Equal(CadenceBuildJobStatus.Pending, job.Status);
        Assert.Equal(CadenceBuildJobType.CaptureSymbol, job.JobType);
    }

    private static AiIntakeController CreateController(ApplicationDbContext dbContext, params string[] roles)
    {
        var controller = new AiIntakeController(
            dbContext,
            new McpLibraryWorkflowService(
                dbContext,
                Options.Create(new CadenceAutomationOptions
                {
                    JobRoot = "storage/jobs",
                    CaptureQueuePath = "storage/jobs/capture",
                    AllegroQueuePath = "storage/jobs/allegro",
                    LibraryRoot = "library/Cadence"
                })),
            new StubAiDatasheetExtractionService(new JsonSchemaValidationService()),
            new LocalPdfTextExtractor());

        IdentityManagementTestHelper.AttachControllerContext(controller, Guid.NewGuid().ToString("N"), "user@test.local", roles);
        controller.TempData = new TempDataDictionary(controller.HttpContext, new TestTempDataProvider());
        return controller;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<AiDatasheetExtraction> SeedExtractionAsync(ApplicationDbContext dbContext, AiDatasheetExtractionStatus status)
    {
        var candidate = new OnlineCandidate
        {
            SourceProvider = "test",
            Manufacturer = "Test Manufacturer",
            ManufacturerPN = "TEST-123",
            CandidateStatus = CandidateStatus.NewFromWeb,
            LifecycleStatus = LifecycleStatus.Unknown
        };
        dbContext.OnlineCandidates.Add(candidate);
        await dbContext.SaveChangesAsync();

        var extraction = new AiDatasheetExtraction
        {
            CandidateId = candidate.Id,
            Manufacturer = candidate.Manufacturer,
            ManufacturerPartNumber = candidate.ManufacturerPN,
            ExtractionJson = "{}",
            SymbolSpecJson = "{}",
            FootprintSpecJson = "{}",
            Confidence = 0.8m,
            Status = status
        };
        dbContext.AiDatasheetExtractions.Add(extraction);
        await dbContext.SaveChangesAsync();
        return extraction;
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
