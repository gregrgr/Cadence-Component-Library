using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class PartAlternateServiceTests
{
    [Fact]
    public async Task ValidateAsync_FailsForAltLevelA_WhenFootprintsDiffer()
    {
        await using var dbContext = CreateDbContext();
        SeedCompanyPart(dbContext, "CP-001", "FOOTPRINT-A", "SYM-A", ApprovalStatus.Approved);
        SeedCompanyPart(dbContext, "CP-002", "FOOTPRINT-B", "SYM-A", ApprovalStatus.Approved);

        var service = new PartAlternateService(dbContext);
        var alternate = new PartAlternate
        {
            SourceCompanyPN = "CP-001",
            TargetCompanyPN = "CP-002",
            AltLevel = AlternateLevel.A
        };

        var result = await service.ValidateAsync(alternate);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("Default Footprint", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateAsync_SucceedsForAltLevelA_WhenFootprintsMatch()
    {
        await using var dbContext = CreateDbContext();
        SeedCompanyPart(dbContext, "CP-001", "FOOTPRINT-A", "SYM-A", ApprovalStatus.Approved);
        SeedCompanyPart(dbContext, "CP-002", "FOOTPRINT-A", "SYM-B", ApprovalStatus.Approved);

        var service = new PartAlternateService(dbContext);
        var alternate = new PartAlternate
        {
            SourceCompanyPN = "CP-001",
            TargetCompanyPN = "CP-002",
            AltLevel = AlternateLevel.A
        };

        var result = await service.ValidateAsync(alternate);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task PrepareForSaveAsync_ComputesCompatibilityFlags()
    {
        await using var dbContext = CreateDbContext();
        SeedCompanyPart(dbContext, "CP-001", "FOOTPRINT-A", "SYM-A", ApprovalStatus.Approved);
        SeedCompanyPart(dbContext, "CP-002", "FOOTPRINT-B", "SYM-B", ApprovalStatus.Approved);

        var service = new PartAlternateService(dbContext);
        var alternate = new PartAlternate
        {
            SourceCompanyPN = "CP-001",
            TargetCompanyPN = "CP-002",
            AltLevel = AlternateLevel.B
        };

        await service.PrepareForSaveAsync(alternate);

        Assert.False(alternate.SameFootprintYN);
        Assert.False(alternate.SameSymbolYN);
        Assert.True(alternate.NeedLayoutReviewYN);
        Assert.True(alternate.NeedEEReviewYN);
    }

    [Fact]
    public async Task PrepareForSaveAsync_SetsMatchingFlags_WhenFootprintAndSymbolMatch()
    {
        await using var dbContext = CreateDbContext();
        SeedCompanyPart(dbContext, "CP-001", "FOOTPRINT-A", "SYM-A", ApprovalStatus.Approved);
        SeedCompanyPart(dbContext, "CP-002", "FOOTPRINT-A", "SYM-A", ApprovalStatus.Approved);

        var service = new PartAlternateService(dbContext);
        var alternate = new PartAlternate
        {
            SourceCompanyPN = "CP-001",
            TargetCompanyPN = "CP-002",
            AltLevel = AlternateLevel.A
        };

        await service.PrepareForSaveAsync(alternate);

        Assert.True(alternate.SameFootprintYN);
        Assert.True(alternate.SameSymbolYN);
        Assert.False(alternate.NeedLayoutReviewYN);
        Assert.False(alternate.NeedEEReviewYN);
    }

    [Fact]
    public async Task ValidateApprovalAsync_Fails_WhenEitherPartIsNotApproved()
    {
        await using var dbContext = CreateDbContext();
        SeedCompanyPart(dbContext, "CP-001", "FOOTPRINT-A", "SYM-A", ApprovalStatus.PendingReview);
        SeedCompanyPart(dbContext, "CP-002", "FOOTPRINT-A", "SYM-A", ApprovalStatus.Approved);

        var service = new PartAlternateService(dbContext);
        var alternate = new PartAlternate
        {
            SourceCompanyPN = "CP-001",
            TargetCompanyPN = "CP-002",
            AltLevel = AlternateLevel.A
        };

        var result = await service.ValidateApprovalAsync(alternate);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("must be approved", StringComparison.Ordinal));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedCompanyPart(
        ApplicationDbContext dbContext,
        string companyPn,
        string footprintName,
        string symbolFamilyCode,
        ApprovalStatus approvalStatus)
    {
        dbContext.CompanyParts.Add(new CompanyPart
        {
            CompanyPN = companyPn,
            PartClass = "Passive",
            Description = companyPn,
            SymbolFamilyCode = symbolFamilyCode,
            PackageFamilyCode = "PKG",
            DefaultFootprintName = footprintName,
            DatasheetUrl = "https://example.test/datasheet.pdf",
            ApprovalStatus = approvalStatus,
            LifecycleStatus = LifecycleStatus.Active
        });

        dbContext.SaveChanges();
    }
}
