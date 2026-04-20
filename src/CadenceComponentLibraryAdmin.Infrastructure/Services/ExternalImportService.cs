using System.Security.Cryptography;
using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class ExternalImportService : IExternalImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ExternalImportOptions _options;

    public ExternalImportService(
        ApplicationDbContext dbContext,
        IOptions<ExternalImportOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<ExternalImportUpsertResult> UpsertEasyEdaComponentAsync(
        EasyEdaComponentImportRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var sourceName = string.IsNullOrWhiteSpace(request.SourceName) ? "EasyEDA Pro" : request.SourceName.Trim();
        var entity = await FindExistingImportAsync(sourceName, request.ExternalDeviceUuid, request.LcscId, cancellationToken)
            ?? new ExternalComponentImport
            {
                SourceName = sourceName,
                CreatedBy = actor
            };

        if (entity.Id == 0)
        {
            _dbContext.ExternalComponentImports.Add(entity);
        }

        entity.ExternalDeviceUuid = Normalize(request.ExternalDeviceUuid);
        entity.ExternalLibraryUuid = Normalize(request.ExternalLibraryUuid);
        entity.SearchKeyword = Normalize(request.SearchKeyword);
        entity.LcscId = Normalize(request.LcscId);
        entity.ImportKey = BuildImportKey(sourceName, entity.ExternalDeviceUuid, entity.LcscId, request.Manufacturer, request.ManufacturerPN);
        entity.Name = Normalize(request.Name);
        entity.Description = Normalize(request.Description);
        entity.ClassificationJson = request.Classification.HasValue
            ? JsonSerializer.Serialize(request.Classification.Value, JsonOptions)
            : null;
        entity.Manufacturer = Normalize(request.Manufacturer);
        entity.ManufacturerPN = Normalize(request.ManufacturerPN);
        entity.Supplier = Normalize(request.Supplier);
        entity.SupplierId = Normalize(request.SupplierId);
        entity.SymbolName = Normalize(request.SymbolName);
        entity.SymbolUuid = Normalize(request.SymbolUuid);
        entity.SymbolLibraryUuid = Normalize(request.SymbolLibraryUuid);
        entity.SymbolType = Normalize(request.SymbolType);
        entity.SymbolRawJson = NormalizeJson(request.SymbolRawJson);
        entity.FootprintName = Normalize(request.FootprintName);
        entity.FootprintUuid = Normalize(request.FootprintUuid);
        entity.FootprintLibraryUuid = Normalize(request.FootprintLibraryUuid);
        entity.FootprintRawJson = NormalizeJson(request.FootprintRawJson);
        entity.ImageUuidsJson = request.ImageUuids is { Count: > 0 } ? JsonSerializer.Serialize(request.ImageUuids, JsonOptions) : null;
        entity.Model3DName = Normalize(request.Model3DName);
        entity.Model3DUuid = Normalize(request.Model3DUuid);
        entity.Model3DLibraryUuid = Normalize(request.Model3DLibraryUuid);
        entity.Model3DRawJson = NormalizeJson(request.Model3DRawJson);
        entity.DatasheetUrl = Normalize(request.DatasheetUrl);
        entity.ManualUrl = Normalize(request.ManualUrl);
        entity.StepUrl = Normalize(request.StepUrl);
        entity.JlcInventory = request.JlcInventory;
        entity.JlcPrice = request.JlcPrice;
        entity.LcscInventory = request.LcscInventory;
        entity.LcscPrice = request.LcscPrice;
        entity.SearchItemRawJson = NormalizeJson(request.SearchItemRawJson);
        entity.DeviceItemRawJson = NormalizeJson(request.DeviceItemRawJson);
        entity.DeviceAssociationRawJson = NormalizeJson(request.DeviceAssociationRawJson);
        entity.DevicePropertyRawJson = NormalizeJson(request.DevicePropertyRawJson);
        entity.OtherPropertyRawJson = NormalizeJson(request.OtherPropertyRawJson);
        entity.FullRawJson = NormalizeJson(request.FullRawJson);
        entity.LastImportedAt = DateTime.UtcNow;
        entity.UpdatedBy = actor;

        var duplicateWarnings = await BuildDuplicateWarningsAsync(entity, cancellationToken);
        entity.DuplicateWarning = duplicateWarnings.Count == 0 ? null : string.Join("; ", duplicateWarnings);
        entity.ImportStatus = duplicateWarnings.Count == 0 ? ExternalImportStatus.Imported : ExternalImportStatus.DuplicateFound;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var missingCriticalFields = BuildMissingCriticalFields(entity);
        var summary = new ExternalImportFieldSummary(
            entity.Name,
            entity.Manufacturer,
            entity.ManufacturerPN,
            entity.Supplier,
            entity.LcscId,
            entity.SymbolName,
            entity.FootprintName,
            entity.Model3DName,
            !string.IsNullOrWhiteSpace(entity.DatasheetUrl) || entity.DatasheetAssetId.HasValue,
            !string.IsNullOrWhiteSpace(entity.ManualUrl) || entity.ManualAssetId.HasValue,
            !string.IsNullOrWhiteSpace(entity.StepUrl) || entity.StepAssetId.HasValue || !string.IsNullOrWhiteSpace(entity.Model3DUuid),
            entity.ImportStatus);

        return new ExternalImportUpsertResult(entity.Id, duplicateWarnings, missingCriticalFields, summary);
    }

    public async Task<ExternalComponentAsset> SaveAssetAsync(
        long importId,
        ExternalImportAssetUpload request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var import = await _dbContext.ExternalComponentImports.FirstOrDefaultAsync(x => x.Id == importId, cancellationToken)
            ?? throw new InvalidOperationException($"External import '{importId}' was not found.");

        if (request.Content is null && string.IsNullOrWhiteSpace(request.Url))
        {
            throw new InvalidOperationException("An uploaded file or source URL is required.");
        }

        var asset = new ExternalComponentAsset
        {
            SourceName = import.SourceName,
            ExternalComponentImportId = import.Id,
            AssetType = request.AssetType,
            ExternalUuid = Normalize(request.ExternalUuid),
            Url = Normalize(request.Url),
            RawMetadataJson = NormalizeJson(request.RawMetadataJson),
            CreatedBy = actor
        };

        if (request.Content is not null)
        {
            var storageRoot = ResolveStorageRoot();
            var importFolder = Path.Combine(storageRoot, import.Id.ToString());
            Directory.CreateDirectory(importFolder);

            var extension = SanitizeExtension(request.FileName);
            var safeName = $"{request.AssetType.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(importFolder, safeName);
            var importFolderFullPath = Path.GetFullPath(importFolder);
            var fullPathResolved = Path.GetFullPath(fullPath);
            if (!fullPathResolved.StartsWith(importFolderFullPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Resolved asset path escaped the configured storage root.");
            }

            await using (var destination = File.Create(fullPathResolved))
            {
                await request.Content.CopyToAsync(destination, cancellationToken);
            }

            await using var hashingStream = File.OpenRead(fullPathResolved);
            var hash = await SHA256.HashDataAsync(hashingStream, cancellationToken);

            asset.FileName = safeName;
            asset.OriginalFileName = request.OriginalFileName ?? request.FileName;
            asset.ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType;
            asset.StoragePath = Path.GetRelativePath(storageRoot, fullPathResolved).Replace('\\', '/');
            asset.Sha256 = Convert.ToHexString(hash).ToLowerInvariant();
            asset.SizeBytes = request.SizeBytes;
        }

        _dbContext.ExternalComponentAssets.Add(asset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await LinkAssetAsync(import, asset, actor, cancellationToken);
        return asset;
    }

    public async Task<OnlineCandidate> CreateCandidateAsync(long importId, string actor, CancellationToken cancellationToken = default)
    {
        var import = await _dbContext.ExternalComponentImports.FirstOrDefaultAsync(x => x.Id == importId, cancellationToken)
            ?? throw new InvalidOperationException($"External import '{importId}' was not found.");

        if (import.CandidateId.HasValue)
        {
            var existingCandidate = await _dbContext.OnlineCandidates.FirstAsync(x => x.Id == import.CandidateId.Value, cancellationToken);
            return existingCandidate;
        }

        var candidate = new OnlineCandidate
        {
            SourceProvider = import.SourceName,
            Manufacturer = import.Manufacturer ?? "Unknown",
            ManufacturerPN = import.ManufacturerPN ?? (import.LcscId ?? import.Name ?? $"easyeda-import-{import.Id}"),
            Description = string.IsNullOrWhiteSpace(import.Description) ? import.Name : import.Description,
            RawPackageName = import.FootprintName,
            DatasheetUrl = import.DatasheetUrl ?? import.ManualUrl ?? import.StepUrl,
            SymbolDownloaded = !string.IsNullOrWhiteSpace(import.SymbolName),
            FootprintDownloaded = !string.IsNullOrWhiteSpace(import.FootprintName),
            StepDownloaded = import.StepAssetId.HasValue || !string.IsNullOrWhiteSpace(import.StepUrl) || !string.IsNullOrWhiteSpace(import.Model3DUuid),
            LifecycleStatus = LifecycleStatus.Unknown,
            CandidateStatus = CandidateStatus.NewFromWeb,
            ImportNote = $"Created from External Import #{import.Id} ({import.SourceName}).",
            CreatedBy = actor
        };

        _dbContext.OnlineCandidates.Add(candidate);
        await _dbContext.SaveChangesAsync(cancellationToken);

        import.ImportStatus = ExternalImportStatus.CandidateCreated;
        import.CandidateId = candidate.Id;
        import.UpdatedBy = actor;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return candidate;
    }

    public async Task RejectImportAsync(long importId, string actor, CancellationToken cancellationToken = default)
    {
        var import = await _dbContext.ExternalComponentImports.FirstOrDefaultAsync(x => x.Id == importId, cancellationToken)
            ?? throw new InvalidOperationException($"External import '{importId}' was not found.");

        import.ImportStatus = ExternalImportStatus.Rejected;
        import.UpdatedBy = actor;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task LinkAssetAsync(
        ExternalComponentImport import,
        ExternalComponentAsset asset,
        string actor,
        CancellationToken cancellationToken)
    {
        switch (asset.AssetType)
        {
            case ExternalComponentAssetType.Thumbnail:
            case ExternalComponentAssetType.FootprintRenderImage:
                import.FootprintRenderAssetId = asset.Id;
                break;
            case ExternalComponentAssetType.Datasheet:
                import.DatasheetAssetId = asset.Id;
                break;
            case ExternalComponentAssetType.Manual:
                import.ManualAssetId = asset.Id;
                break;
            case ExternalComponentAssetType.Step:
                import.StepAssetId = asset.Id;
                break;
        }

        import.UpdatedBy = actor;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ExternalComponentImport?> FindExistingImportAsync(
        string sourceName,
        string? externalDeviceUuid,
        string? lcscId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(externalDeviceUuid))
        {
            return await _dbContext.ExternalComponentImports
                .FirstOrDefaultAsync(
                    x => x.SourceName == sourceName && x.ExternalDeviceUuid == externalDeviceUuid,
                    cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(lcscId))
        {
            return await _dbContext.ExternalComponentImports
                .FirstOrDefaultAsync(
                    x => x.SourceName == sourceName && x.LcscId == lcscId,
                    cancellationToken);
        }

        return null;
    }

    private async Task<IReadOnlyList<string>> BuildDuplicateWarningsAsync(
        ExternalComponentImport import,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();

        if (!string.IsNullOrWhiteSpace(import.Manufacturer) && !string.IsNullOrWhiteSpace(import.ManufacturerPN))
        {
            var manufacturerPartExists = await _dbContext.ManufacturerParts
                .AnyAsync(
                    x => x.Manufacturer == import.Manufacturer && x.ManufacturerPN == import.ManufacturerPN,
                    cancellationToken);
            if (manufacturerPartExists)
            {
                warnings.Add("Manufacturer + ManufacturerPN already exists in ManufacturerParts.");
            }
        }

        if (!string.IsNullOrWhiteSpace(import.LcscId))
        {
            var duplicateLcscImports = await _dbContext.ExternalComponentImports
                .CountAsync(x => x.SourceName == import.SourceName && x.LcscId == import.LcscId && x.Id != import.Id, cancellationToken);
            if (duplicateLcscImports > 0)
            {
                warnings.Add("Another staged import already uses the same LCSC ID.");
            }
        }

        return warnings;
    }

    private static IReadOnlyList<string> BuildMissingCriticalFields(ExternalComponentImport import)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(import.Name))
        {
            missing.Add("name");
        }

        if (string.IsNullOrWhiteSpace(import.Manufacturer))
        {
            missing.Add("manufacturer");
        }

        if (string.IsNullOrWhiteSpace(import.ManufacturerPN))
        {
            missing.Add("manufacturerPN");
        }

        if (string.IsNullOrWhiteSpace(import.SymbolUuid) && string.IsNullOrWhiteSpace(import.SymbolName))
        {
            missing.Add("symbol");
        }

        if (string.IsNullOrWhiteSpace(import.FootprintUuid) && string.IsNullOrWhiteSpace(import.FootprintName))
        {
            missing.Add("footprint");
        }

        return missing;
    }

    private static string BuildImportKey(
        string sourceName,
        string? externalDeviceUuid,
        string? lcscId,
        string? manufacturer,
        string? manufacturerPn)
    {
        if (!string.IsNullOrWhiteSpace(externalDeviceUuid))
        {
            return $"{sourceName}:{externalDeviceUuid.Trim()}";
        }

        if (!string.IsNullOrWhiteSpace(lcscId))
        {
            return $"{sourceName}:lcsc:{lcscId.Trim()}";
        }

        return $"{sourceName}:{Normalize(manufacturer)}:{Normalize(manufacturerPn)}".TrimEnd(':');
    }

    private string ResolveStorageRoot()
    {
        var configuredRoot = string.IsNullOrWhiteSpace(_options.StorageRoot)
            ? "App_Data/ExternalImports"
            : _options.StorageRoot!;

        var resolved = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredRoot));
        Directory.CreateDirectory(resolved);
        return resolved;
    }

    private static string SanitizeExtension(string? fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return ".bin";
        }

        var sanitized = new string(extension
            .Where(ch => char.IsLetterOrDigit(ch) || ch == '.')
            .Take(16)
            .ToArray());

        if (string.IsNullOrWhiteSpace(sanitized) || sanitized == ".")
        {
            return ".bin";
        }

        return sanitized.StartsWith(".", StringComparison.Ordinal) ? sanitized : $".{sanitized}";
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeJson(string? value)
    {
        var normalized = Normalize(value);
        if (normalized is null)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(normalized);
            return JsonSerializer.Serialize(document.RootElement, JsonOptions);
        }
        catch (JsonException)
        {
            return normalized;
        }
    }
}
