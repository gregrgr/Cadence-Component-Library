using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class NlbnEasyEdaClient : INlbnEasyEdaClient
{
    private const string SourceName = "EasyEDA/LCSC";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IExternalImportService _externalImportService;
    private readonly ExternalImportOptions _options;

    public NlbnEasyEdaClient(
        HttpClient httpClient,
        ApplicationDbContext dbContext,
        IExternalImportService externalImportService,
        IOptions<ExternalImportOptions> options)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _externalImportService = externalImportService;
        _options = options.Value;
    }

    public async Task<NlbnComponentFetchResult> FetchComponentAsync(string lcscId, CancellationToken ct)
    {
        var normalizedLcscId = NormalizeLcscId(lcscId);
        EnsureEnabled();

        using var response = await SendWithRetryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Get,
                BuildComponentRequestPath(normalizedLcscId, _options.EasyEdaNlbn.ComponentVersion)),
            ct);

        var rawJson = await response.Content.ReadAsStringAsync(ct);
        using var document = JsonDocument.Parse(rawJson);
        var result = GetResultElement(document.RootElement);

        var title = GetString(result, "title");
        var description = GetString(result, "description");

        var dataStrElement = ParsePossiblyEmbeddedJson(result, "dataStr");
        var packageDetailElement = ParsePossiblyEmbeddedJson(result, "packageDetail");
        var lcscElement = ParsePossiblyEmbeddedJson(result, "lcsc");
        var packageDataStrElement = packageDetailElement.HasValue
            ? ParsePossiblyEmbeddedJson(packageDetailElement.Value, "dataStr")
            : null;
        var cParaElement = dataStrElement.HasValue
            ? GetNestedElement(dataStrElement.Value, "head", "c_para")
            : null;

        var cPara = cParaElement.HasValue
            ? ToStringDictionary(cParaElement.Value)
            : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var manufacturer = FirstNonEmpty(
            cPara.TryGetValue("BOM_Manufacturer", out var bomManufacturer) ? bomManufacturer : null,
            cPara.TryGetValue("Manufacturer", out var manufacturerText) ? manufacturerText : null,
            GetString(result, "brand"));

        var manufacturerPn = FirstNonEmpty(
            FindByPossibleKey(cPara, "BOM_Manufacturer Part", "BOM_Manufacturer Part Number", "Manufacturer Part Number", "MPN", "mf"),
            title);

        var packageName = FirstNonEmpty(
            FindByPossibleKey(cPara, "package", "Package", "PackageName"),
            GetString(packageDetailElement, "title"),
            GetString(result, "package"));

        var lcscUrl = GetString(lcscElement, "url");
        var lcscNumericId = GetString(lcscElement, "id");
        var fallbackUrl = BuildLcscFallbackUrl(title, lcscNumericId);

        var symbolShapeJson = ExtractShapeJson(dataStrElement, "shape");
        var footprintShapeJson = ExtractShapeJson(packageDataStrElement, "shape");
        var (modelUuid, modelName) = ExtractOutline3DInfo(footprintShapeJson);

        return new NlbnComponentFetchResult
        {
            LcscId = normalizedLcscId,
            Title = title,
            Description = description,
            Manufacturer = manufacturer,
            ManufacturerPnCandidate = manufacturerPn,
            PackageName = packageName,
            DatasheetUrl = FirstNonEmpty(lcscUrl, fallbackUrl),
            ManualUrl = GetString(lcscElement, "manualUrl"),
            JlcPartClass = FindByPossibleKey(cPara, "BOM_JLCPCB Part Class"),
            Model3DUuid = modelUuid,
            Model3DName = modelName,
            SymbolShapeJson = symbolShapeJson,
            FootprintShapeJson = footprintShapeJson,
            SymbolBBoxX = GetDecimal(dataStrElement, "head", "x"),
            SymbolBBoxY = GetDecimal(dataStrElement, "head", "y"),
            FootprintBBoxX = GetDecimal(packageDataStrElement, "head", "x"),
            FootprintBBoxY = GetDecimal(packageDataStrElement, "head", "y"),
            EasyEdaRawJson = NormalizeJson(rawJson),
            EasyEdaDataStrRawJson = SerializeElement(dataStrElement),
            EasyEdaPackageDetailRawJson = SerializeElement(packageDetailElement),
            EasyEdaLcscRawJson = SerializeElement(lcscElement),
            EasyEdaCParaJson = SerializeElement(cParaElement)
        };
    }

    public async Task<ExternalComponentImport> ImportByLcscIdAsync(
        string lcscId,
        NlbnImportOptions options,
        string actor,
        CancellationToken ct)
    {
        var fetch = await FetchComponentAsync(lcscId, ct);
        var entity = await _dbContext.ExternalComponentImports
            .FirstOrDefaultAsync(x => x.SourceName == SourceName && x.LcscId == fetch.LcscId, ct)
            ?? new ExternalComponentImport
            {
                SourceName = SourceName,
                CreatedBy = actor
            };

        if (entity.Id == 0)
        {
            _dbContext.ExternalComponentImports.Add(entity);
        }

        entity.LcscId = fetch.LcscId;
        entity.ImportKey = $"{SourceName}:lcsc:{fetch.LcscId}";
        entity.Name = Normalize(fetch.Title);
        entity.Description = Normalize(fetch.Description);
        entity.Manufacturer = Normalize(fetch.Manufacturer);
        entity.ManufacturerPN = Normalize(fetch.ManufacturerPnCandidate);
        entity.PackageName = Normalize(fetch.PackageName);
        entity.FootprintName = Normalize(fetch.PackageName);
        entity.Supplier = "LCSC";
        entity.SupplierId = fetch.LcscId;
        entity.JlcPartClass = Normalize(fetch.JlcPartClass);
        entity.SymbolShapeJson = NormalizeJson(fetch.SymbolShapeJson);
        entity.SymbolRawJson = entity.SymbolShapeJson;
        entity.SymbolBBoxX = fetch.SymbolBBoxX;
        entity.SymbolBBoxY = fetch.SymbolBBoxY;
        entity.FootprintShapeJson = NormalizeJson(fetch.FootprintShapeJson);
        entity.FootprintRawJson = entity.FootprintShapeJson;
        entity.FootprintBBoxX = fetch.FootprintBBoxX;
        entity.FootprintBBoxY = fetch.FootprintBBoxY;
        entity.Model3DUuid = Normalize(fetch.Model3DUuid);
        entity.Model3DName = Normalize(fetch.Model3DName);
        entity.DatasheetUrl = Normalize(fetch.DatasheetUrl);
        entity.ManualUrl = Normalize(fetch.ManualUrl) ?? entity.DatasheetUrl;
        entity.FullRawJson = NormalizeJson(fetch.EasyEdaRawJson);
        entity.EasyEdaRawJson = NormalizeJson(fetch.EasyEdaRawJson);
        entity.EasyEdaDataStrRawJson = NormalizeJson(fetch.EasyEdaDataStrRawJson);
        entity.EasyEdaPackageDetailRawJson = NormalizeJson(fetch.EasyEdaPackageDetailRawJson);
        entity.EasyEdaLcscRawJson = NormalizeJson(fetch.EasyEdaLcscRawJson);
        entity.EasyEdaCParaJson = NormalizeJson(fetch.EasyEdaCParaJson);
        entity.LastImportedAt = DateTime.UtcNow;
        entity.UpdatedBy = actor;

        var duplicateWarnings = await BuildDuplicateWarningsAsync(entity, ct);
        entity.DuplicateWarning = duplicateWarnings.Count == 0 ? null : string.Join("; ", duplicateWarnings);
        entity.ImportStatus = duplicateWarnings.Count == 0
            ? ExternalImportStatus.Imported
            : ExternalImportStatus.DuplicateFound;

        await _dbContext.SaveChangesAsync(ct);

        if (ShouldGeneratePreview(options))
        {
            try
            {
                await GenerateFootprintPreviewAsync(entity.Id, ct);
            }
            catch
            {
                // Preview generation is best-effort and must not block the staging import.
            }
        }

        if (!string.IsNullOrWhiteSpace(entity.Model3DUuid) && ShouldDownloadStep(options))
        {
            try
            {
                await DownloadStepAsync(entity.Id, entity.Model3DUuid, ct);
            }
            catch
            {
                // STEP download is optional in this milestone.
            }
        }

        if (!string.IsNullOrWhiteSpace(entity.Model3DUuid) && ShouldDownloadObj(options))
        {
            try
            {
                await DownloadObjAsync(entity.Id, entity.Model3DUuid, ct);
            }
            catch
            {
                // OBJ download is optional in this milestone.
            }
        }

        return entity;
    }

    public async Task<ExternalComponentAsset?> DownloadStepAsync(
        long externalComponentImportId,
        string modelUuid,
        CancellationToken ct)
    {
        var import = await GetImportAsync(externalComponentImportId, ct);
        if (string.IsNullOrWhiteSpace(modelUuid))
        {
            return null;
        }

        var url = BuildStepUrl(modelUuid);
        var bytes = await DownloadBytesAsync(url, ct);
        if (bytes.Length == 0)
        {
            return null;
        }

        await using var stream = new MemoryStream(bytes);
        var asset = await _externalImportService.SaveAssetAsync(
            import.Id,
            new ExternalImportAssetUpload(
                ExternalComponentAssetType.Step,
                stream,
                $"{import.LcscId ?? modelUuid}.step",
                $"{import.LcscId ?? modelUuid}.step",
                "application/step",
                bytes.LongLength,
                modelUuid,
                url,
                "{\"source\":\"EasyEDA/LCSC STEP\"}"),
            "easyeda-nlbn",
            ct);

        import.StepUrl = url;
        import.UpdatedBy = "easyeda-nlbn";
        await _dbContext.SaveChangesAsync(ct);
        return asset;
    }

    public async Task<ExternalComponentAsset?> DownloadObjAsync(
        long externalComponentImportId,
        string modelUuid,
        CancellationToken ct)
    {
        var import = await GetImportAsync(externalComponentImportId, ct);
        if (string.IsNullOrWhiteSpace(modelUuid))
        {
            return null;
        }

        var url = BuildObjUrl(modelUuid);
        var bytes = await DownloadBytesAsync(url, ct);
        if (bytes.Length == 0)
        {
            return null;
        }

        await using var stream = new MemoryStream(bytes);
        var asset = await _externalImportService.SaveAssetAsync(
            import.Id,
            new ExternalImportAssetUpload(
                ExternalComponentAssetType.Obj,
                stream,
                $"{import.LcscId ?? modelUuid}.obj",
                $"{import.LcscId ?? modelUuid}.obj",
                "model/obj",
                bytes.LongLength,
                modelUuid,
                url,
                "{\"source\":\"EasyEDA/LCSC OBJ\"}"),
            "easyeda-nlbn",
            ct);

        import.UpdatedBy = "easyeda-nlbn";
        await _dbContext.SaveChangesAsync(ct);
        return asset;
    }

    public async Task<ExternalComponentAsset?> GenerateFootprintPreviewAsync(
        long externalComponentImportId,
        CancellationToken ct)
    {
        var import = await GetImportAsync(externalComponentImportId, ct);
        var svg = BuildPreviewSvg(import);
        var bytes = Encoding.UTF8.GetBytes(svg);

        await using var stream = new MemoryStream(bytes);
        var asset = await _externalImportService.SaveAssetAsync(
            import.Id,
            new ExternalImportAssetUpload(
                ExternalComponentAssetType.FootprintPreview,
                stream,
                $"{import.LcscId ?? import.Id.ToString(CultureInfo.InvariantCulture)}-preview.svg",
                $"{import.LcscId ?? import.Id.ToString(CultureInfo.InvariantCulture)}-preview.svg",
                "image/svg+xml",
                bytes.LongLength,
                null,
                null,
                "{\"source\":\"EasyEDA/LCSC preview\"}"),
            "easyeda-nlbn",
            ct);

        return asset;
    }

    private async Task<ExternalComponentImport> GetImportAsync(long externalComponentImportId, CancellationToken ct)
    {
        return await _dbContext.ExternalComponentImports
            .FirstOrDefaultAsync(x => x.Id == externalComponentImportId, ct)
            ?? throw new InvalidOperationException($"External import '{externalComponentImportId}' was not found.");
    }

    private async Task<byte[]> DownloadBytesAsync(string url, CancellationToken ct)
    {
        using var response = await SendWithRetryAsync(
            () => new HttpRequestMessage(HttpMethod.Get, url),
            ct);

        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken ct)
    {
        Exception? lastError = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            if (_options.EasyEdaNlbn.RequestDelayMs > 0)
            {
                await Task.Delay(_options.EasyEdaNlbn.RequestDelayMs * (attempt == 1 ? 1 : attempt), ct);
            }

            try
            {
                using var request = requestFactory();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    response.Dispose();
                    throw new InvalidOperationException($"LCSC ID '{request.RequestUri}' was not found in EasyEDA.");
                }

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                var body = await response.Content.ReadAsStringAsync(ct);
                response.Dispose();
                throw new HttpRequestException(
                    $"EasyEDA request failed with status {(int)response.StatusCode}: {body}",
                    null,
                    response.StatusCode);
            }
            catch (Exception ex) when (attempt < 3 && ex is not InvalidOperationException)
            {
                lastError = ex;
                await Task.Delay(TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1)), ct);
            }
            catch (Exception ex)
            {
                lastError = ex;
                break;
            }
        }

        throw new InvalidOperationException("EasyEDA request failed after 3 attempts.", lastError);
    }

    private async Task<IReadOnlyList<string>> BuildDuplicateWarningsAsync(
        ExternalComponentImport import,
        CancellationToken ct)
    {
        var warnings = new List<string>();

        if (!string.IsNullOrWhiteSpace(import.Manufacturer) && !string.IsNullOrWhiteSpace(import.ManufacturerPN))
        {
            var manufacturerPartExists = await _dbContext.ManufacturerParts
                .AnyAsync(
                    x => x.Manufacturer == import.Manufacturer && x.ManufacturerPN == import.ManufacturerPN,
                    ct);
            if (manufacturerPartExists)
            {
                warnings.Add("Manufacturer + ManufacturerPN already exists in ManufacturerParts.");
            }
        }

        if (!string.IsNullOrWhiteSpace(import.LcscId))
        {
            var duplicateLcscImports = await _dbContext.ExternalComponentImports
                .CountAsync(
                    x => x.SourceName == import.SourceName && x.LcscId == import.LcscId && x.Id != import.Id,
                    ct);
            if (duplicateLcscImports > 0)
            {
                warnings.Add("Another staged import already uses the same LCSC ID.");
            }
        }

        return warnings;
    }

    private static string BuildComponentRequestPath(string lcscId, string componentVersion)
        => $"/api/products/{Uri.EscapeDataString(lcscId)}/components?version={Uri.EscapeDataString(componentVersion)}";

    private string BuildStepUrl(string modelUuid)
        => $"{_options.EasyEdaNlbn.ModulesBaseUrl.TrimEnd('/')}/{_options.EasyEdaNlbn.StepPathPrefix.Trim('/')}/{Uri.EscapeDataString(modelUuid)}";

    private string BuildObjUrl(string modelUuid)
        => $"{_options.EasyEdaNlbn.ModulesBaseUrl.TrimEnd('/')}/3dmodel/{Uri.EscapeDataString(modelUuid)}";

    private bool ShouldDownloadStep(NlbnImportOptions options)
        => options.DownloadStep ?? _options.EasyEdaNlbn.DownloadStepByDefault;

    private bool ShouldDownloadObj(NlbnImportOptions options)
        => options.DownloadObj ?? _options.EasyEdaNlbn.DownloadObjByDefault;

    private bool ShouldGeneratePreview(NlbnImportOptions options)
        => options.GeneratePreview ?? _options.EasyEdaNlbn.GeneratePreviewByDefault;

    private void EnsureEnabled()
    {
        if (!_options.EasyEdaNlbn.Enabled)
        {
            throw new InvalidOperationException("EasyEDA/LCSC nlbn-style import is disabled.");
        }
    }

    private static string NormalizeLcscId(string lcscId)
    {
        var value = Normalize(lcscId) ?? throw new ArgumentException("LCSC ID is required.", nameof(lcscId));
        return value.StartsWith("C", StringComparison.OrdinalIgnoreCase) ? value.ToUpperInvariant() : $"C{value}";
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, JsonOptions);
        }
        catch (JsonException)
        {
            return json.Trim();
        }
    }

    private static JsonElement GetResultElement(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("result", out var result))
        {
            return result;
        }

        return root;
    }

    private static JsonElement? ParsePossiblyEmbeddedJson(JsonElement parent, string propertyName)
    {
        if (parent.ValueKind != JsonValueKind.Object || !parent.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return ParsePossiblyEmbeddedJson(value);
    }

    private static JsonElement? ParsePossiblyEmbeddedJson(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var raw = value.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            try
            {
                return JsonDocument.Parse(raw).RootElement.Clone();
            }
            catch (JsonException)
            {
                return value.Clone();
            }
        }

        return value.Clone();
    }

    private static string? ExtractShapeJson(JsonElement? parent, string propertyName)
    {
        if (!parent.HasValue || parent.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!parent.Value.TryGetProperty(propertyName, out var shape))
        {
            return null;
        }

        return SerializeElement(ParsePossiblyEmbeddedJson(shape) ?? shape.Clone());
    }

    private static string? SerializeElement(JsonElement? element)
    {
        if (!element.HasValue)
        {
            return null;
        }

        return JsonSerializer.Serialize(element.Value, JsonOptions);
    }

    private static string? GetString(JsonElement? element, string propertyName)
    {
        if (!element.HasValue || element.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!element.Value.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => Normalize(value.GetString()),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => Normalize(value.GetRawText())
        };
    }

    private static JsonElement? GetNestedElement(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return ParsePossiblyEmbeddedJson(current) ?? current.Clone();
    }

    private static decimal? GetDecimal(JsonElement? element, params string[] path)
    {
        if (!element.HasValue)
        {
            return null;
        }

        var nested = GetNestedElement(element.Value, path);
        if (!nested.HasValue)
        {
            return null;
        }

        return nested.Value.ValueKind switch
        {
            JsonValueKind.Number when nested.Value.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(nested.Value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }

    private static Dictionary<string, string?> ToStringDictionary(JsonElement element)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (element.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => property.Value.GetRawText()
            };
        }

        return result;
    }

    private static string? FindByPossibleKey(
        IReadOnlyDictionary<string, string?> values,
        params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static (string? Model3DUuid, string? Model3DName) ExtractOutline3DInfo(string? footprintShapeJson)
    {
        if (string.IsNullOrWhiteSpace(footprintShapeJson))
        {
            return (null, null);
        }

        foreach (var line in ExtractShapeLines(footprintShapeJson))
        {
            if (!line.StartsWith("SVGNODE~", StringComparison.Ordinal))
            {
                continue;
            }

            var payload = line["SVGNODE~".Length..];
            try
            {
                using var document = JsonDocument.Parse(payload);
                var attrs = GetNestedElement(document.RootElement, "attrs");
                if (!attrs.HasValue)
                {
                    continue;
                }

                var cEtype = GetString(attrs, "c_etype");
                if (!string.Equals(cEtype, "outline3D", StringComparison.Ordinal))
                {
                    continue;
                }

                return (GetString(attrs, "uuid"), GetString(attrs, "title"));
            }
            catch (JsonException)
            {
                continue;
            }
        }

        return (null, null);
    }

    private static IEnumerable<string> ExtractShapeLines(string rawShapeJson)
    {
        var lines = new List<string>();

        try
        {
            using var document = JsonDocument.Parse(rawShapeJson);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                    {
                        lines.Add(item.GetString()!);
                    }
                }

                return lines;
            }

            if (root.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(root.GetString()))
            {
                return SplitShapeLines(root.GetString()!).ToList();
            }
        }
        catch (JsonException)
        {
            // Fall back to plain text splitting below.
        }

        return SplitShapeLines(rawShapeJson).ToList();
    }

    private static IEnumerable<string> SplitShapeLines(string raw)
    {
        return raw
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line));
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static string? BuildLcscFallbackUrl(string? title, string? lcscNumericId)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(lcscNumericId))
        {
            return null;
        }

        var safeTitle = Uri.EscapeDataString(title.Trim());
        return $"https://item.szlcsc.com/datasheet/{safeTitle}/{lcscNumericId.Trim()}.html";
    }

    private static string BuildPreviewSvg(ExternalComponentImport import)
    {
        var title = SecurityElementEscape(import.Name ?? "EasyEDA/LCSC Import");
        var lcscId = SecurityElementEscape(import.LcscId ?? "Unknown");
        var packageName = SecurityElementEscape(import.PackageName ?? import.FootprintName ?? "Unknown");
        var shapeCount = CountShapeLines(import.FootprintShapeJson);
        var previewState = shapeCount > 0
            ? $"Raw footprint shape count: {shapeCount}"
            : "Preview unavailable";
        var previewStateEscaped = SecurityElementEscape(previewState);

        return $$"""
        <svg xmlns="http://www.w3.org/2000/svg" width="640" height="360" viewBox="0 0 640 360">
          <rect width="640" height="360" fill="#f6f3ea"/>
          <rect x="24" y="24" width="592" height="312" rx="18" fill="#ffffff" stroke="#d7cfbe" stroke-width="2"/>
          <text x="48" y="74" font-size="28" font-family="Segoe UI, Arial, sans-serif" fill="#1f2937">{{title}}</text>
          <text x="48" y="118" font-size="22" font-family="Consolas, monospace" fill="#92400e">LCSC ID: {{lcscId}}</text>
          <text x="48" y="156" font-size="20" font-family="Segoe UI, Arial, sans-serif" fill="#374151">Package: {{packageName}}</text>
          <text x="48" y="220" font-size="24" font-family="Segoe UI, Arial, sans-serif" fill="#b45309">{{previewStateEscaped}}</text>
          <text x="48" y="276" font-size="16" font-family="Consolas, monospace" fill="#6b7280">Staging-only preview. No Allegro PSM/DRA conversion in Milestone B4.</text>
        </svg>
        """;
    }

    private static int CountShapeLines(string? shapeJson)
        => string.IsNullOrWhiteSpace(shapeJson) ? 0 : ExtractShapeLines(shapeJson).Count();

    private static string SecurityElementEscape(string value)
        => value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);
}
