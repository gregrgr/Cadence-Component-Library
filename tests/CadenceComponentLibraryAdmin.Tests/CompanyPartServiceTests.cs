using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class CompanyPartServiceTests
{
    [Fact]
    public async Task ValidateApprovalAsync_ReturnsErrorsForMissingApprovedDependencies()
    {
        await using var dbContext = CreateDbContext();
        var service = new CompanyPartService(dbContext, new NoOpChangeLogService());

        var companyPart = new CompanyPart
        {
            CompanyPN = "CP-001",
            PartClass = "Passive",
            Description = "10k resistor",
            SymbolFamilyCode = "RES",
            PackageFamilyCode = "0402",
            DefaultFootprintName = "RES_0402",
            ApprovalStatus = ApprovalStatus.Approved,
            LifecycleStatus = LifecycleStatus.Active
        };

        var result = await service.ValidateApprovalAsync(companyPart);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("Datasheet URL", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.Contains("approved Manufacturer Part", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.Contains("valid Symbol Family", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.Contains("valid Package Family", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.Contains("valid Default Footprint", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateApprovalAsync_SucceedsWhenApprovedDependenciesExist()
    {
        await using var dbContext = CreateDbContext();
        SeedValidApprovalGraph(dbContext);

        var service = new CompanyPartService(dbContext, new NoOpChangeLogService());
        var companyPart = new CompanyPart
        {
            CompanyPN = "CP-001",
            PartClass = "Passive",
            Description = "10k resistor",
            SymbolFamilyCode = "RES",
            PackageFamilyCode = "0402",
            DefaultFootprintName = "RES_0402",
            DatasheetUrl = "https://example.test/resistor.pdf",
            ApprovalStatus = ApprovalStatus.Approved,
            LifecycleStatus = LifecycleStatus.Active
        };

        var result = await service.ValidateApprovalAsync(companyPart);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedValidApprovalGraph(ApplicationDbContext dbContext)
    {
        dbContext.SymbolFamilies.Add(new SymbolFamily
        {
            SymbolFamilyCode = "RES",
            SymbolName = "RES_2PIN",
            OlbPath = "Symbols_OLB/Passive/passive.olb",
            PartClass = "Passive",
            IsActive = true
        });

        dbContext.PackageFamilies.Add(new PackageFamily
        {
            PackageFamilyCode = "0402",
            MountType = "SMD",
            LeadCount = 2,
            PackageSignature = "SMD|2|1.00|0.50|0.50|0.00|0.00"
        });

        dbContext.FootprintVariants.Add(new FootprintVariant
        {
            FootprintName = "RES_0402",
            PackageFamilyCode = "0402",
            Status = FootprintStatus.Released,
            PsmPath = "Footprints/RES_0402.psm",
            DraPath = "Footprints/RES_0402.dra",
            VariantType = "Production"
        });

        dbContext.ManufacturerParts.Add(new ManufacturerPart
        {
            CompanyPN = "CP-001",
            Manufacturer = "ACME",
            ManufacturerPN = "ACME-10K-0402",
            LifecycleStatus = LifecycleStatus.Active,
            IsApproved = true
        });

        dbContext.SaveChanges();
    }
}
