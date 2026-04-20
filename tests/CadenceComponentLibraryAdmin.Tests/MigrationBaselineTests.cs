using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class MigrationBaselineTests
{
    [Fact]
    public void InitialCreateMigration_Exists()
    {
        using var dbContext = CreateSqlServerMetadataDbContext();

        var migrations = dbContext.Database.GetMigrations().ToList();

        Assert.Contains(migrations, migration => migration.EndsWith("InitialCreate", StringComparison.Ordinal));
    }

    [Fact]
    public void CisViewScript_IsCopiedToTestOutput()
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Data", "Views", "CisViews.sql");

        Assert.True(File.Exists(scriptPath), $"Expected CIS view script at '{scriptPath}'.");
    }

    [Fact]
    public async Task MigrateAsync_CreatesDatabaseObjectsAndViews_WhenSqlServerConnectionIsAvailable()
    {
        await RunAgainstFreshSqlServerDatabase(async dbContext =>
        {
            await dbContext.Database.MigrateAsync();

            var tables = await dbContext.Database
                .SqlQueryRaw<string>("SELECT TABLE_NAME AS [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'")
                .ToListAsync();

            Assert.Contains("CompanyParts", tables);
            Assert.Contains("ManufacturerParts", tables);
            Assert.Contains("SymbolFamilies", tables);
            Assert.Contains("PackageFamilies", tables);
            Assert.Contains("FootprintVariants", tables);
            Assert.Contains("OnlineCandidates", tables);
            Assert.Contains("SupplierOffers", tables);
            Assert.Contains("PartAlternates", tables);
            Assert.Contains("PartDocs", tables);
            Assert.Contains("PartChangeLogs", tables);
            Assert.Contains("LibraryReleases", tables);
            Assert.Contains("AspNetUsers", tables);
            Assert.Contains("AspNetRoles", tables);

            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
            Assert.Contains(appliedMigrations, migration => migration.EndsWith("InitialCreate", StringComparison.Ordinal));

            var views = await DatabaseBootstrapper.GetInstalledViewsAsync(dbContext);
            Assert.Contains("vw_CIS_Release_Parts", views);
            Assert.Contains("vw_CIS_Alternates", views);
            Assert.Contains("ExternalImportSources", tables);
            Assert.Contains("ExternalComponentImports", tables);
            Assert.Contains("ExternalComponentAssets", tables);
        });
    }

    [Fact]
    public async Task InitializeAsync_KeepsCisAlternatesViewAvailable()
    {
        await RunAgainstFreshSqlServerDatabase(async dbContext =>
        {
            await DatabaseBootstrapper.InitializeAsync(dbContext);

            var views = await DatabaseBootstrapper.GetInstalledViewsAsync(dbContext);
            Assert.Contains("vw_CIS_Alternates", views);
        });
    }

    [Fact]
    public async Task PackageSignature_UniqueConstraint_IsEnforcedBySqlServer()
    {
        await RunAgainstFreshSqlServerDatabase(async dbContext =>
        {
            await dbContext.Database.MigrateAsync();

            dbContext.PackageFamilies.Add(new PackageFamily
            {
                PackageFamilyCode = "0402-A",
                MountType = "SMD",
                LeadCount = 2,
                PackageSignature = "SMD|2|1.00|0.50|0.50|0.00|0.00"
            });
            await dbContext.SaveChangesAsync();

            dbContext.PackageFamilies.Add(new PackageFamily
            {
                PackageFamilyCode = "0402-B",
                MountType = "SMD",
                LeadCount = 2,
                PackageSignature = "SMD|2|1.00|0.50|0.50|0.00|0.00"
            });

            await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        });
    }

    [Fact]
    public async Task ManufacturerAndManufacturerPn_UniqueConstraint_IsEnforcedBySqlServer()
    {
        await RunAgainstFreshSqlServerDatabase(async dbContext =>
        {
            await dbContext.Database.MigrateAsync();
            await SeedCompanyPartGraphAsync(dbContext, "CP-001");

            dbContext.ManufacturerParts.Add(new ManufacturerPart
            {
                CompanyPN = "CP-001",
                Manufacturer = "ACME",
                ManufacturerPN = "ACME-10K-0402",
                LifecycleStatus = LifecycleStatus.Active,
                IsApproved = true
            });
            await dbContext.SaveChangesAsync();

            dbContext.ManufacturerParts.Add(new ManufacturerPart
            {
                CompanyPN = "CP-001",
                Manufacturer = "ACME",
                ManufacturerPN = "ACME-10K-0402",
                LifecycleStatus = LifecycleStatus.Active
            });

            await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        });
    }

    private static ApplicationDbContext CreateSqlServerMetadataDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=CadenceComponentLibrary;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task RunAgainstFreshSqlServerDatabase(Func<ApplicationDbContext, Task> assertion)
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            return;
        }

        var databaseName = $"CadenceMigrationTests_{Guid.NewGuid():N}";
        var connectionString = $"{baseConnectionString};Initial Catalog={databaseName}";
        await using var dbContext = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options);

        try
        {
            await assertion(dbContext);
        }
        finally
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    private static async Task SeedCompanyPartGraphAsync(ApplicationDbContext dbContext, string companyPn)
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
            PsmPath = "Footprints/RES_0402.psm",
            DraPath = "Footprints/RES_0402.dra",
            VariantType = "Production",
            Status = FootprintStatus.Released
        });

        dbContext.CompanyParts.Add(new CompanyPart
        {
            CompanyPN = companyPn,
            PartClass = "Passive",
            Description = "10k resistor",
            SymbolFamilyCode = "RES",
            PackageFamilyCode = "0402",
            DefaultFootprintName = "RES_0402",
            DatasheetUrl = "https://example.test/resistor.pdf",
            ApprovalStatus = ApprovalStatus.Approved,
            LifecycleStatus = LifecycleStatus.Active
        });

        await dbContext.SaveChangesAsync();
    }
}
