using System.Security.Claims;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Controllers;
using CadenceComponentLibraryAdmin.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class EasyEdaImportControllerTests : IDisposable
{
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "cadence-easyeda-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task MissingImportToken_ReturnsUnauthorized()
    {
        await using var dbContext = CreateDbContext();
        var controller = CreateApiController(dbContext);

        var result = await controller.ImportComponent(CreateRequest(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task InvalidImportToken_ReturnsUnauthorized()
    {
        await using var dbContext = CreateDbContext();
        var controller = CreateApiController(dbContext, tokenHeader: "wrong-token");

        var result = await controller.ImportComponent(CreateRequest(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ExpiredImportToken_ReturnsUnauthorized()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = CreateTokenService(dbContext);
        var created = await tokenService.CreateTokenAsync(
            new ExternalImportTokenCreateRequest("expired", "EasyEDA Pro", DateTime.UtcNow.AddMinutes(-5), null, null),
            "user-1",
            "admin@local.test");
        var controller = CreateApiController(dbContext, tokenHeader: created.RawToken);

        var result = await controller.ImportComponent(CreateRequest(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RevokedImportToken_ReturnsUnauthorized()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = CreateTokenService(dbContext);
        var created = await tokenService.CreateTokenAsync(
            new ExternalImportTokenCreateRequest("revoked", "EasyEDA Pro", DateTime.UtcNow.AddDays(1), null, null),
            "user-1",
            "admin@local.test");
        await tokenService.RevokeTokenAsync(created.Token.Id, "user-2", "admin@local.test");
        var controller = CreateApiController(dbContext, tokenHeader: created.RawToken);

        var result = await controller.ImportComponent(CreateRequest(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ValidImportToken_CreatesExternalComponentImport()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = CreateTokenService(dbContext);
        var created = await tokenService.CreateTokenAsync(
            new ExternalImportTokenCreateRequest("valid", "EasyEDA Pro", DateTime.UtcNow.AddDays(30), null, null),
            "user-1",
            "admin@local.test");
        var controller = CreateApiController(dbContext, tokenHeader: created.RawToken);

        var result = await controller.ImportComponent(CreateRequest(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var entity = await dbContext.ExternalComponentImports.SingleAsync();
        Assert.Equal("EasyEDA Pro", entity.SourceName);
        Assert.Equal("dev-001", entity.ExternalDeviceUuid);
        Assert.Equal("ACME", entity.Manufacturer);
        Assert.Equal("{}", entity.ClassificationJson);
        Assert.Equal(0, await dbContext.CompanyParts.CountAsync());
    }

    [Fact]
    public async Task LastUsedAt_IsUpdatedAfterSuccessfulImport()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = CreateTokenService(dbContext);
        var created = await tokenService.CreateTokenAsync(
            new ExternalImportTokenCreateRequest("last-used", "EasyEDA Pro", DateTime.UtcNow.AddDays(30), null, null),
            "user-1",
            "admin@local.test");
        var controller = CreateApiController(dbContext, tokenHeader: created.RawToken);

        await controller.ImportComponent(CreateRequest(), CancellationToken.None);

        var entity = await dbContext.ExternalImportTokens.SingleAsync();
        Assert.True(entity.LastUsedAt.HasValue);
    }

    [Fact]
    public async Task RepeatedImport_UpsertsInsteadOfDuplicating()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");
        await service.UpsertEasyEdaComponentAsync(CreateRequest() with { Description = "Updated" }, "tester");

        Assert.Equal(1, await dbContext.ExternalComponentImports.CountAsync());
        Assert.Equal("Updated", (await dbContext.ExternalComponentImports.SingleAsync()).Description);
    }

    [Fact]
    public async Task DuplicateLcscImport_IsDetected()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");
        var result = await service.UpsertEasyEdaComponentAsync(
            CreateRequest() with
            {
                ExternalDeviceUuid = "dev-002",
                Description = "duplicate lcsc"
            },
            "tester");

        Assert.Contains(result.DuplicateWarnings, warning => warning.Contains("same LCSC ID", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DuplicateManufacturerPart_WritesWarning()
    {
        await using var dbContext = CreateDbContext();
        await SeedManufacturerPartGraphAsync(dbContext);
        var service = CreateService(dbContext);

        var result = await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");

        Assert.Contains(result.DuplicateWarnings, warning => warning.Contains("Manufacturer + ManufacturerPN", StringComparison.Ordinal));
        Assert.Equal(ExternalImportStatus.DuplicateFound, (await dbContext.ExternalComponentImports.SingleAsync()).ImportStatus);
    }

    [Fact]
    public async Task RawJsonFields_ArePreserved()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        await service.UpsertEasyEdaComponentAsync(CreateRequest() with
        {
            SymbolRawJson = "{\"uuid\":\"sym-001\"}",
            FootprintRawJson = "{\"uuid\":\"fpt-001\"}",
            SearchItemRawJson = "{\"name\":\"search\"}",
            DeviceItemRawJson = "{\"name\":\"device\"}",
            DeviceAssociationRawJson = "{\"symbol\":{\"uuid\":\"sym-001\"}}",
            DevicePropertyRawJson = "{\"supplier\":\"LCSC\"}",
            OtherPropertyRawJson = "{\"datasheet\":\"https://example.test/datasheet.pdf\"}",
            FullRawJson = "{\"all\":true}"
        }, "tester");

        var entity = await dbContext.ExternalComponentImports.SingleAsync();
        Assert.Contains("\"all\": true", entity.FullRawJson ?? string.Empty);
        Assert.Contains("\"datasheet\"", entity.OtherPropertyRawJson ?? string.Empty);
        Assert.Contains("\"supplier\"", entity.DevicePropertyRawJson ?? string.Empty);
    }

    [Fact]
    public async Task AssetUpload_StoresFileAndSha256()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var import = await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("render-image"));
        var formFile = new FormFile(stream, 0, stream.Length, "file", "render.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var asset = await service.SaveAssetAsync(
            import.ImportId,
            new ExternalImportAssetUpload(
                ExternalComponentAssetType.Other,
                formFile.OpenReadStream(),
                formFile.FileName,
                formFile.FileName,
                formFile.ContentType,
                formFile.Length,
                "asset-001",
                null,
                "{\"kind\":\"test\"}"),
            "tester");

        Assert.NotNull(asset.Sha256);
        Assert.True(asset.SizeBytes > 0);
        Assert.False(string.IsNullOrWhiteSpace(asset.StoragePath));
        Assert.StartsWith($"{import.ImportId}/", asset.StoragePath, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(_storageRoot, asset.StoragePath!.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task ThumbnailUpload_LinksFootprintRenderAsset()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var import = await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var formFile = new FormFile(stream, 0, stream.Length, "file", "thumb.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var asset = await service.SaveAssetAsync(
            import.ImportId,
            new ExternalImportAssetUpload(
                ExternalComponentAssetType.Thumbnail,
                formFile.OpenReadStream(),
                formFile.FileName,
                formFile.FileName,
                formFile.ContentType,
                formFile.Length,
                null,
                null,
                null),
            "tester");
        var entity = await dbContext.ExternalComponentImports.SingleAsync();

        Assert.Equal(asset.Id, entity.FootprintRenderAssetId);
    }

    [Fact]
    public async Task StepUpload_LinksStepAsset()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var import = await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");
        await using var stream = new MemoryStream(new byte[] { 9, 8, 7 });
        var formFile = new FormFile(stream, 0, stream.Length, "file", "model.step")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/step"
        };

        var asset = await service.SaveAssetAsync(
            import.ImportId,
            new ExternalImportAssetUpload(
                ExternalComponentAssetType.Step,
                formFile.OpenReadStream(),
                formFile.FileName,
                formFile.FileName,
                formFile.ContentType,
                formFile.Length,
                null,
                null,
                null),
            "tester");
        var entity = await dbContext.ExternalComponentImports.SingleAsync();

        Assert.Equal(asset.Id, entity.StepAssetId);
    }

    [Fact]
    public async Task AssetUpload_UsesConfiguredStorageRoot_AndSanitizesFileName()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var import = await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var formFile = new FormFile(stream, 0, stream.Length, "file", "..\\..\\secret/unsafe?.step.exe")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };

        var asset = await service.SaveAssetAsync(
            import.ImportId,
            new ExternalImportAssetUpload(
                ExternalComponentAssetType.Step,
                formFile.OpenReadStream(),
                formFile.FileName,
                formFile.FileName,
                formFile.ContentType,
                formFile.Length,
                null,
                null,
                null),
            "tester");

        var resolvedPath = Path.GetFullPath(Path.Combine(_storageRoot, asset.StoragePath!.Replace('/', Path.DirectorySeparatorChar)));
        Assert.StartsWith(Path.GetFullPath(_storageRoot), resolvedPath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(".exe", Path.GetExtension(asset.FileName));
    }

    [Fact]
    public async Task CreateCandidate_CreatesOnlineCandidateWithoutApprovingCompanyPart()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var import = await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");

        var candidate = await service.CreateCandidateAsync(import.ImportId, "tester");

        Assert.Equal(CandidateStatus.NewFromWeb, candidate.CandidateStatus);
        Assert.Equal(0, await dbContext.CompanyParts.CountAsync());
        var entity = await dbContext.ExternalComponentImports.SingleAsync();
        Assert.Equal(ExternalImportStatus.CandidateCreated, entity.ImportStatus);
        Assert.Equal(candidate.Id, entity.CandidateId);
    }

    [Fact]
    public async Task EnrichFromLcsc_WhenDisabled_DoesNotCallRemoteAndStoresDisabledStatus()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var import = await service.UpsertEasyEdaComponentAsync(CreateRequest(), "tester");

        var result = await service.EnrichFromLcscAsync(import.ImportId, "tester");

        Assert.False(result.Success);
        var entity = await dbContext.ExternalComponentImports.SingleAsync();
        Assert.Equal(LcscEnrichmentStatus.Disabled, entity.LcscEnrichmentStatus);
    }

    [Fact]
    public void ExternalImportsPage_RequiresLogin()
    {
        var authorizeAttribute = Assert.Single(typeof(ExternalImportsController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
        Assert.IsType<AuthorizeAttribute>(authorizeAttribute);
    }

    [Fact]
    public void CreateCandidate_Endpoint_RequiresAuthenticatedRole()
    {
        var method = typeof(EasyEdaImportController).GetMethod(nameof(EasyEdaImportController.CreateCandidate));
        Assert.NotNull(method);
        var authorizeAttribute = Assert.Single(method!.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
        var typedAttribute = Assert.IsType<AuthorizeAttribute>(authorizeAttribute);
        Assert.Contains("Admin", typedAttribute.Roles ?? string.Empty, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, recursive: true);
        }
    }

    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private ExternalImportService CreateService(ApplicationDbContext dbContext)
    {
        Directory.CreateDirectory(_storageRoot);
        return new ExternalImportService(
            dbContext,
            Options.Create(new ExternalImportOptions
            {
                EasyEdaApiKey = "test-key",
                StorageRoot = _storageRoot
            }),
            new StubLcscOpenApiClient(),
            Options.Create(new LcscOpenApiOptions()));
    }

    private ExternalImportTokenService CreateTokenService(ApplicationDbContext dbContext)
        => new(dbContext);

    private EasyEdaImportController CreateApiController(ApplicationDbContext dbContext, string? tokenHeader = null, string? apiKeyHeader = null)
    {
        var controller = new EasyEdaImportController(
            CreateService(dbContext),
            CreateTokenService(dbContext),
            Options.Create(new ExternalImportOptions
            {
                EasyEdaApiKey = "test-key",
                StorageRoot = _storageRoot
            }),
            new FakeHostEnvironment());

        var httpContext = new DefaultHttpContext();
        if (!string.IsNullOrWhiteSpace(tokenHeader))
        {
            httpContext.Request.Headers["X-Import-Token"] = tokenHeader;
        }

        if (!string.IsNullOrWhiteSpace(apiKeyHeader))
        {
            httpContext.Request.Headers["X-Import-Api-Key"] = apiKeyHeader;
        }

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "admin@local.test"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static EasyEdaComponentImportRequest CreateRequest()
        => new()
        {
            SourceName = "EasyEDA Pro",
            ExternalDeviceUuid = "dev-001",
            ExternalLibraryUuid = "lib-001",
            SearchKeyword = "opamp",
            LcscId = "C2040",
            Name = "LM358",
            Description = "Dual op-amp",
            Classification = System.Text.Json.JsonDocument.Parse("{}").RootElement.Clone(),
            Manufacturer = "ACME",
            ManufacturerPN = "LM358-ACME",
            Supplier = "LCSC",
            SupplierId = "LCSC-1",
            SymbolName = "LM358_SYM",
            SymbolUuid = "sym-001",
            SymbolLibraryUuid = "sym-lib-001",
            FootprintName = "SOIC-8",
            FootprintUuid = "fpt-001",
            FootprintLibraryUuid = "fpt-lib-001",
            Model3DName = "SOIC-8-3D",
            Model3DUuid = "3d-001",
            Model3DLibraryUuid = "3d-lib-001",
            DatasheetUrl = "https://example.test/datasheet.pdf",
            ManualUrl = "https://example.test/manual.pdf",
            StepUrl = "https://example.test/model.step",
            SearchItemRawJson = "{\"uuid\":\"dev-001\"}",
            DeviceItemRawJson = "{\"uuid\":\"dev-001\",\"property\":{}}",
            DeviceAssociationRawJson = "{\"symbol\":{\"uuid\":\"sym-001\"}}",
            DevicePropertyRawJson = "{\"manufacturer\":\"ACME\"}",
            OtherPropertyRawJson = "{\"datasheet\":\"https://example.test/datasheet.pdf\"}",
            FullRawJson = "{\"searchItem\":{\"uuid\":\"dev-001\"}}"
        };

    private static async Task SeedManufacturerPartGraphAsync(ApplicationDbContext dbContext)
    {
        dbContext.SymbolFamilies.Add(new SymbolFamily
        {
            SymbolFamilyCode = "OPAMP",
            SymbolName = "OPAMP",
            OlbPath = "Symbols/opamp.olb",
            PartClass = "IC",
            IsActive = true
        });

        dbContext.PackageFamilies.Add(new PackageFamily
        {
            PackageFamilyCode = "SOIC8",
            MountType = "SMD",
            LeadCount = 8,
            PackageSignature = "SMD|8|5.00|4.00|1.27|0.00|0.00"
        });

        dbContext.FootprintVariants.Add(new FootprintVariant
        {
            FootprintName = "SOIC-8",
            PackageFamilyCode = "SOIC8",
            PsmPath = "Footprints/SOIC-8.psm",
            DraPath = "Footprints/SOIC-8.dra",
            VariantType = "Production",
            Status = FootprintStatus.Released
        });

        dbContext.CompanyParts.Add(new CompanyPart
        {
            CompanyPN = "CP-IMPORT-1",
            PartClass = "IC",
            Description = "Dual op-amp",
            SymbolFamilyCode = "OPAMP",
            PackageFamilyCode = "SOIC8",
            DefaultFootprintName = "SOIC-8",
            ApprovalStatus = ApprovalStatus.Approved,
            LifecycleStatus = LifecycleStatus.Active
        });

        dbContext.ManufacturerParts.Add(new ManufacturerPart
        {
            CompanyPN = "CP-IMPORT-1",
            Manufacturer = "ACME",
            ManufacturerPN = "LM358-ACME",
            LifecycleStatus = LifecycleStatus.Active,
            IsApproved = true
        });

        await dbContext.SaveChangesAsync();
    }

    private sealed class StubLcscOpenApiClient : ILcscOpenApiClient
    {
        public bool IsEnabled => false;

        public Task<LcscSearchProductsResponse> SearchProductsAsync(string keyword, int page, int pageSize, string exactOrFuzzy, CancellationToken cancellationToken = default)
            => Task.FromResult(new LcscSearchProductsResponse(false, "disabled", null, []));

        public Task<LcscProductInfoResponse> GetProductInfoAsync(string productNumber, CancellationToken cancellationToken = default)
            => Task.FromResult(new LcscProductInfoResponse(false, "disabled", null, null, null, null, null, null, null, null, null, null));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
