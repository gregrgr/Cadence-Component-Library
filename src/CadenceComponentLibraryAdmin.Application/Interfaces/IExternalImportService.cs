using System.Text.Json;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IExternalImportService
{
    Task<ExternalImportUpsertResult> UpsertEasyEdaComponentAsync(
        EasyEdaComponentImportRequest request,
        string actor,
        CancellationToken cancellationToken = default);

    Task<ExternalComponentAsset> SaveAssetAsync(
        long importId,
        ExternalImportAssetUpload request,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OnlineCandidate> CreateCandidateAsync(
        long importId,
        string actor,
        CancellationToken cancellationToken = default);

    Task RejectImportAsync(
        long importId,
        string actor,
        CancellationToken cancellationToken = default);

    Task<ExternalImportEnrichmentResult> EnrichFromLcscAsync(
        long importId,
        string actor,
        CancellationToken cancellationToken = default);
}

public sealed record EasyEdaComponentImportRequest
{
    public string SourceName { get; init; } = "EasyEDA Pro";
    public string? ExternalDeviceUuid { get; init; }
    public string? ExternalLibraryUuid { get; init; }
    public string? SearchKeyword { get; init; }
    public string? LcscId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public JsonElement? Classification { get; init; }
    public string? Manufacturer { get; init; }
    public string? ManufacturerPN { get; init; }
    public string? Supplier { get; init; }
    public string? SupplierId { get; init; }
    public string? SymbolName { get; init; }
    public string? SymbolUuid { get; init; }
    public string? SymbolLibraryUuid { get; init; }
    public string? SymbolType { get; init; }
    public string? SymbolRawJson { get; init; }
    public string? FootprintName { get; init; }
    public string? FootprintUuid { get; init; }
    public string? FootprintLibraryUuid { get; init; }
    public string? FootprintRawJson { get; init; }
    public IReadOnlyList<string>? ImageUuids { get; init; }
    public string? Model3DName { get; init; }
    public string? Model3DUuid { get; init; }
    public string? Model3DLibraryUuid { get; init; }
    public string? Model3DRawJson { get; init; }
    public string? DatasheetUrl { get; init; }
    public string? ManualUrl { get; init; }
    public string? StepUrl { get; init; }
    public long? JlcInventory { get; init; }
    public decimal? JlcPrice { get; init; }
    public long? LcscInventory { get; init; }
    public decimal? LcscPrice { get; init; }
    public string? SearchItemRawJson { get; init; }
    public string? DeviceItemRawJson { get; init; }
    public string? DeviceAssociationRawJson { get; init; }
    public string? DevicePropertyRawJson { get; init; }
    public string? OtherPropertyRawJson { get; init; }
    public string? FullRawJson { get; init; }
}

public sealed record ExternalImportUpsertResult(
    long ImportId,
    IReadOnlyList<string> DuplicateWarnings,
    IReadOnlyList<string> MissingCriticalFields,
    ExternalImportFieldSummary Summary);

public sealed record ExternalImportFieldSummary(
    string? Name,
    string? Manufacturer,
    string? ManufacturerPN,
    string? Supplier,
    string? LcscId,
    string? SymbolName,
    string? FootprintName,
    string? Model3DName,
    bool HasDatasheet,
    bool HasManual,
    bool HasStep,
    ExternalImportStatus ImportStatus);

public sealed record ExternalImportAssetUpload(
    ExternalComponentAssetType AssetType,
    Stream? Content,
    string? FileName,
    string? OriginalFileName,
    string? ContentType,
    long? SizeBytes,
    string? ExternalUuid,
    string? Url,
    string? RawMetadataJson);

public sealed record ExternalImportEnrichmentResult(
    bool Success,
    string Message);
