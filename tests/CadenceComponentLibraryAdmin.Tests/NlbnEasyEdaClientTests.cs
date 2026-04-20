using System.Net;
using System.Net.Http;
using System.Security.Claims;
using CadenceComponentLibraryAdmin.Application.Interfaces;
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

public sealed class NlbnEasyEdaClientTests : IDisposable
{
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "cadence-easyeda-nlbn-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task FetchComponentAsync_BuildsCorrectUrl_AndMapsCoreFields()
    {
        await using var dbContext = CreateDbContext();
        var handler = new RecordingHandler(LoadJson("component-with-3d.json"));
        var client = CreateClient(dbContext, handler);

        var result = await client.FetchComponentAsync("C2040", CancellationToken.None);

        Assert.Equal("/api/products/C2040/components?version=6.4.19.5", handler.Requests.Single().PathAndQuery);
        Assert.Equal("USBLC6-2SC6", result.Title);
        Assert.Equal("ESD protection array", result.Description);
        Assert.Equal("STMicroelectronics", result.Manufacturer);
        Assert.Equal("SOT-23-6", result.PackageName);
        Assert.Equal("https://item.szlcsc.com/123456.html", result.DatasheetUrl);
        Assert.Contains("P~0 0 100 100", result.SymbolShapeJson ?? string.Empty);
        Assert.Contains("PAD~1~10~10", result.FootprintShapeJson ?? string.Empty);
        Assert.Contains("BOM_JLCPCB Part Class", result.EasyEdaCParaJson ?? string.Empty);
        Assert.Equal("3d-uuid-001", result.Model3DUuid);
        Assert.Equal("SOT-23-6 Body", result.Model3DName);
    }

    [Fact]
    public async Task ImportByLcscIdAsync_CreatesOrUpdatesSingleStagingRecord()
    {
        await using var dbContext = CreateDbContext();
        var handler = new RecordingHandler(LoadJson("component-with-3d.json"));
        var client = CreateClient(dbContext, handler);

        var first = await client.ImportByLcscIdAsync("C2040", new NlbnImportOptions(false, false, false), "tester", CancellationToken.None);
        var second = await client.ImportByLcscIdAsync("C2040", new NlbnImportOptions(false, false, false), "tester", CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await dbContext.ExternalComponentImports.CountAsync());
        var entity = await dbContext.ExternalComponentImports.SingleAsync();
        Assert.Equal("EasyEDA/LCSC", entity.SourceName);
        Assert.Equal("C2040", entity.LcscId);
        Assert.NotNull(entity.EasyEdaRawJson);
        Assert.NotNull(entity.SymbolShapeJson);
        Assert.NotNull(entity.FootprintShapeJson);
        Assert.Equal(ExternalImportStatus.Imported, entity.ImportStatus);
    }

    [Fact]
    public async Task ImportByLcscIdAsync_MissingPackageDetail_DoesNotFail_AndBuildsFallbackUrl()
    {
        await using var dbContext = CreateDbContext();
        var handler = new RecordingHandler(LoadJson("component-missing-package-detail.json"));
        var client = CreateClient(dbContext, handler);

        var entity = await client.ImportByLcscIdAsync("7890", new NlbnImportOptions(false, false, false), "tester", CancellationToken.None);

        Assert.Equal("C7890", entity.LcscId);
        Assert.Equal("Murata", entity.Manufacturer);
        Assert.Equal("0603", entity.PackageName);
        Assert.Null(entity.Model3DUuid);
        Assert.Equal("https://item.szlcsc.com/datasheet/C0603/7890.html", entity.DatasheetUrl);
    }

    [Fact]
    public async Task DownloadStepAndObjAsync_BuildCorrectUrls_AndLinkAssets()
    {
        await using var dbContext = CreateDbContext();
        var handler = new RecordingHandler(
            LoadJson("component-with-3d.json"),
            "step-bytes",
            "obj-bytes");
        var client = CreateClient(dbContext, handler);

        var entity = await client.ImportByLcscIdAsync("C2040", new NlbnImportOptions(false, false, false), "tester", CancellationToken.None);
        var step = await client.DownloadStepAsync(entity.Id, "3d-uuid-001", CancellationToken.None);
        var obj = await client.DownloadObjAsync(entity.Id, "3d-uuid-001", CancellationToken.None);

        Assert.Contains(handler.Requests, request => request.AbsoluteUri == "https://modules.easyeda.com/qAxj6KHrDKw4blvCG8QJPs7Y/3d-uuid-001");
        Assert.Contains(handler.Requests, request => request.AbsoluteUri == "https://modules.easyeda.com/3dmodel/3d-uuid-001");
        Assert.NotNull(step);
        Assert.NotNull(obj);

        var stored = await dbContext.ExternalComponentImports.SingleAsync();
        Assert.Equal(step!.Id, stored.StepAssetId);
        Assert.Equal(obj!.Id, stored.ObjAssetId);
    }

    [Fact]
    public async Task GenerateFootprintPreviewAsync_CreatesPreviewAsset()
    {
        await using var dbContext = CreateDbContext();
        var handler = new RecordingHandler(LoadJson("component-with-3d.json"));
        var client = CreateClient(dbContext, handler);
        var entity = await client.ImportByLcscIdAsync("C2040", new NlbnImportOptions(false, false, true), "tester", CancellationToken.None);

        var stored = await dbContext.ExternalComponentImports.SingleAsync();

        Assert.NotNull(entity);
        Assert.True(stored.FootprintPreviewAssetId.HasValue || stored.FootprintRenderAssetId.HasValue);
        var asset = await dbContext.ExternalComponentAssets.SingleAsync(x => x.AssetType == ExternalComponentAssetType.FootprintPreview);
        Assert.Equal("image/svg+xml", asset.ContentType);
    }

    [Fact]
    public async Task CreateCandidate_MapsParsedNlbnData_AndRemainsStagingOnly()
    {
        await using var dbContext = CreateDbContext();
        var handler = new RecordingHandler(LoadJson("component-with-3d.json"));
        var externalImportService = CreateExternalImportService(dbContext);
        var client = CreateClient(dbContext, handler, externalImportService);

        var entity = await client.ImportByLcscIdAsync("C2040", new NlbnImportOptions(false, false, false), "tester", CancellationToken.None);
        var candidate = await externalImportService.CreateCandidateAsync(entity.Id, "tester");

        Assert.Equal("EasyEDA/LCSC nlbn-style", candidate.SourceProvider);
        Assert.Equal("STMicroelectronics", candidate.Manufacturer);
        Assert.Equal("USBLC6-2SC6", candidate.ManufacturerPN);
        Assert.Equal("SOT-23-6", candidate.RawPackageName);
        Assert.True(candidate.SymbolDownloaded);
        Assert.True(candidate.FootprintDownloaded);
        Assert.True(candidate.StepDownloaded);
        Assert.Equal(CandidateStatus.NewFromWeb, candidate.CandidateStatus);
        Assert.Equal(0, await dbContext.CompanyParts.CountAsync());
    }

    [Fact]
    public void DeprecatedDocs_AreNotPresentedAsCurrentFlow()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepoRoot(), "README.md"));
        var importDoc = File.ReadAllText(Path.Combine(GetRepoRoot(), "docs", "EASYEDA_IMPORT.md"));
        var extensionReadme = File.ReadAllText(Path.Combine(GetRepoRoot(), "integrations", "easyeda-pro-import-extension", "README.md"));

        Assert.DoesNotContain("EasyEDA Pro SDK connector description", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EasyEDA/LCSC", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nlbn-style", importDoc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Deprecated", extensionReadme, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BatchImport_ContinuesOnError_WhenEnabled()
    {
        await using var dbContext = CreateDbContext();
        var controller = CreateController(
            dbContext,
            new StubNlbnEasyEdaClient(async (lcscId, _, _, _) =>
            {
                if (lcscId.Equals("CFAIL", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("bad component");
                }

                var entity = new ExternalComponentImport
                {
                    SourceName = "EasyEDA/LCSC",
                    LcscId = lcscId,
                    ImportKey = $"EasyEDA/LCSC:lcsc:{lcscId}",
                    Name = lcscId,
                    CreatedBy = "tester",
                    ImportStatus = ExternalImportStatus.Imported,
                    LastImportedAt = DateTime.UtcNow
                };
                dbContext.ExternalComponentImports.Add(entity);
                await dbContext.SaveChangesAsync();
                return entity;
            }));

        var result = await controller.BatchImportFromLcsc(
            new ExternalImportBatchInputModel
            {
                LcscIds = "C1000\r\nCFAIL\r\nC2000",
                ContinueOnError = true,
                MaxParallelImports = 2,
                GeneratePreview = false
            },
            CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(2, await dbContext.ExternalComponentImports.CountAsync());
        Assert.Contains("Imported 2 item(s)", controller.TempData["SuccessMessage"]?.ToString(), StringComparison.Ordinal);
        Assert.Contains("CFAIL", controller.TempData["ErrorMessage"]?.ToString(), StringComparison.Ordinal);
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

    private NlbnEasyEdaClient CreateClient(
        ApplicationDbContext dbContext,
        HttpMessageHandler handler,
        IExternalImportService? externalImportService = null)
    {
        Directory.CreateDirectory(_storageRoot);
        return new NlbnEasyEdaClient(
            new HttpClient(handler)
            {
                BaseAddress = new Uri("https://easyeda.com"),
                Timeout = TimeSpan.FromSeconds(30)
            },
            dbContext,
            externalImportService ?? CreateExternalImportService(dbContext),
            Options.Create(new ExternalImportOptions
            {
                EasyEdaApiKey = "test-key",
                StorageRoot = _storageRoot,
                EasyEdaNlbn = new EasyEdaNlbnOptions()
            }));
    }

    private ExternalImportService CreateExternalImportService(ApplicationDbContext dbContext)
    {
        Directory.CreateDirectory(_storageRoot);
        return new ExternalImportService(
            dbContext,
            Options.Create(new ExternalImportOptions
            {
                EasyEdaApiKey = "test-key",
                StorageRoot = _storageRoot,
                EasyEdaNlbn = new EasyEdaNlbnOptions()
            }));
    }

    private ExternalImportsController CreateController(ApplicationDbContext dbContext, INlbnEasyEdaClient client)
    {
        var controller = new ExternalImportsController(
            dbContext,
            CreateExternalImportService(dbContext),
            client,
            Options.Create(new ExternalImportOptions
            {
                EasyEdaApiKey = "test-key",
                StorageRoot = _storageRoot,
                EasyEdaNlbn = new EasyEdaNlbnOptions()
            }))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, "admin@local.test"),
                        new Claim(ClaimTypes.Role, "Admin")
                    ], "TestAuth"))
                }
            },
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new DictionaryTempDataProvider())
        };

        return controller;
    }

    private static string LoadJson(string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "EasyEdaNlbn", fileName));

    private static string GetRepoRoot()
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private sealed class RecordingHandler(params string[] responses) : HttpMessageHandler
    {
        private readonly Queue<string> _responses = new(responses);
        private readonly string _fallbackResponse = responses.LastOrDefault() ?? "{}";

        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            var content = _responses.Count > 0 ? _responses.Dequeue() : _fallbackResponse;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }

    private sealed class StubNlbnEasyEdaClient(
        Func<string, NlbnImportOptions, string, CancellationToken, Task<ExternalComponentImport>> importByLcsc)
        : INlbnEasyEdaClient
    {
        public Task<NlbnComponentFetchResult> FetchComponentAsync(string lcscId, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<ExternalComponentImport> ImportByLcscIdAsync(string lcscId, NlbnImportOptions options, string actor, CancellationToken ct)
            => importByLcsc(lcscId, options, actor, ct);

        public Task<ExternalComponentAsset?> DownloadStepAsync(long externalComponentImportId, string modelUuid, CancellationToken ct)
            => Task.FromResult<ExternalComponentAsset?>(null);

        public Task<ExternalComponentAsset?> DownloadObjAsync(long externalComponentImportId, string modelUuid, CancellationToken ct)
            => Task.FromResult<ExternalComponentAsset?>(null);

        public Task<ExternalComponentAsset?> GenerateFootprintPreviewAsync(long externalComponentImportId, CancellationToken ct)
            => Task.FromResult<ExternalComponentAsset?>(null);
    }

    private sealed class DictionaryTempDataProvider : ITempDataProvider
    {
        private Dictionary<string, object> _data = [];

        public IDictionary<string, object> LoadTempData(HttpContext context) => _data;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            => _data = new Dictionary<string, object>(values);
    }
}
