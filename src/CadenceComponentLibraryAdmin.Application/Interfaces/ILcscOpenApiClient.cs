using System.Text.Json;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface ILcscOpenApiClient
{
    bool IsEnabled { get; }

    Task<LcscSearchProductsResponse> SearchProductsAsync(
        string keyword,
        int page,
        int pageSize,
        string exactOrFuzzy,
        CancellationToken cancellationToken = default);

    Task<LcscProductInfoResponse> GetProductInfoAsync(
        string productNumber,
        CancellationToken cancellationToken = default);
}

public sealed record LcscSearchProductsResponse(
    bool Success,
    string? Message,
    string? RawJson,
    IReadOnlyList<LcscSearchProductItem> Products);

public sealed record LcscSearchProductItem(
    string? ProductNumber,
    string? ManufacturerPartNumber,
    string? Manufacturer,
    long? StockNumber,
    decimal? LowestPrice);

public sealed record LcscProductInfoResponse(
    bool Success,
    string? Message,
    string? RawJson,
    string? ProductNumber,
    string? ManufacturerPartNumber,
    string? Manufacturer,
    string? ProductDescEn,
    string? DatasheetUrl,
    string? ProductImages,
    long? StockNumber,
    decimal? LowestPrice,
    JsonElement? Result);
