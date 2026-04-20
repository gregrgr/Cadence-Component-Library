using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class PackageFamilyServiceTests
{
    [Fact]
    public async Task PrepareForSaveAsync_RejectsDuplicatePackageSignature()
    {
        await using var dbContext = CreateDbContext();
        dbContext.PackageFamilies.Add(new PackageFamily
        {
            PackageFamilyCode = "0402-A",
            MountType = "SMD",
            LeadCount = 2,
            BodyLmm = 1.00m,
            BodyWmm = 0.50m,
            PitchMm = 0.50m,
            EPLmm = 0.00m,
            EPWmm = 0.00m,
            PackageSignature = "SMD|2|1.00|0.50|0.50|0.00|0.00"
        });
        await dbContext.SaveChangesAsync();

        var service = new PackageFamilyService(dbContext);
        var incoming = new PackageFamily
        {
            PackageFamilyCode = "0402-B",
            MountType = "SMD",
            LeadCount = 2,
            BodyLmm = 1.00m,
            BodyWmm = 0.50m,
            PitchMm = 0.50m,
            EPLmm = 0.00m,
            EPWmm = 0.00m
        };

        var result = await service.PrepareForSaveAsync(incoming);

        Assert.False(result.Succeeded);
        Assert.Equal("SMD|2|1.00|0.50|0.50|0.00|0.00", incoming.PackageSignature);
        Assert.Contains(result.Errors, error => error.Contains("Package Signature already exists", StringComparison.Ordinal));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
