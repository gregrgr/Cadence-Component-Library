using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface INlbnEasyEdaClient
{
    Task<NlbnComponentFetchResult> FetchComponentAsync(string lcscId, CancellationToken ct);

    Task<ExternalComponentImport> ImportByLcscIdAsync(
        string lcscId,
        NlbnImportOptions options,
        string actor,
        CancellationToken ct);

    Task<ExternalComponentAsset?> DownloadStepAsync(
        long externalComponentImportId,
        string modelUuid,
        CancellationToken ct);

    Task<ExternalComponentAsset?> DownloadObjAsync(
        long externalComponentImportId,
        string modelUuid,
        CancellationToken ct);

    Task<ExternalComponentAsset?> GenerateFootprintPreviewAsync(
        long externalComponentImportId,
        CancellationToken ct);
}

public sealed record NlbnImportOptions(
    bool? DownloadStep = null,
    bool? DownloadObj = null,
    bool? GeneratePreview = null);

public sealed record NlbnComponentFetchResult
{
    public string LcscId { get; init; } = null!;
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Manufacturer { get; init; }
    public string? ManufacturerPnCandidate { get; init; }
    public string? PackageName { get; init; }
    public string? DatasheetUrl { get; init; }
    public string? ManualUrl { get; init; }
    public string? JlcPartClass { get; init; }
    public string? Model3DUuid { get; init; }
    public string? Model3DName { get; init; }
    public string? SymbolShapeJson { get; init; }
    public string? FootprintShapeJson { get; init; }
    public decimal? SymbolBBoxX { get; init; }
    public decimal? SymbolBBoxY { get; init; }
    public decimal? FootprintBBoxX { get; init; }
    public decimal? FootprintBBoxY { get; init; }
    public string? EasyEdaRawJson { get; init; }
    public string? EasyEdaDataStrRawJson { get; init; }
    public string? EasyEdaPackageDetailRawJson { get; init; }
    public string? EasyEdaLcscRawJson { get; init; }
    public string? EasyEdaCParaJson { get; init; }
}
