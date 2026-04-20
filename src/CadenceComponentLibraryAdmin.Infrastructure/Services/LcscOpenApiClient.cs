using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class LcscOpenApiClient : ILcscOpenApiClient
{
    private readonly HttpClient _httpClient;
    private readonly LcscOpenApiOptions _options;
    private readonly ILogger<LcscOpenApiClient> _logger;

    public LcscOpenApiClient(
        HttpClient httpClient,
        IOptions<LcscOpenApiOptions> options,
        ILogger<LcscOpenApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsEnabled =>
        _options.Enabled &&
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !string.IsNullOrWhiteSpace(_options.ApiSecret);

    public async Task<LcscSearchProductsResponse> SearchProductsAsync(
        string keyword,
        int page,
        int pageSize,
        string exactOrFuzzy,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return new LcscSearchProductsResponse(false, "LCSC Open API enrichment is disabled.", null, []);
        }

        var parameters = CreateSignedParameters();
        parameters["keyword"] = keyword;
        parameters["current_page"] = page.ToString(CultureInfo.InvariantCulture);
        parameters["page_size"] = pageSize.ToString(CultureInfo.InvariantCulture);
        parameters["match_type"] = string.Equals(exactOrFuzzy, "fuzzy", StringComparison.OrdinalIgnoreCase) ? "fuzzy" : "exact";
        parameters["currency"] = _options.Currency;

        var uri = BuildUri("/rest/wmsc2agent/search/product", parameters);
        return await SendSearchAsync(uri, cancellationToken);
    }

    public async Task<LcscProductInfoResponse> GetProductInfoAsync(string productNumber, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return new LcscProductInfoResponse(false, "LCSC Open API enrichment is disabled.", null, null, null, null, null, null, null, null, null, null);
        }

        var parameters = CreateSignedParameters();
        parameters["currency"] = _options.Currency;

        var uri = BuildUri($"/rest/wmsc2agent/product/info/{Uri.EscapeDataString(productNumber)}", parameters);
        return await SendProductInfoAsync(uri, cancellationToken);
    }

    public static string GenerateSignature(string apiKey, string nonce, string apiSecret, string timestamp)
    {
        var payload = $"key={apiKey}&nonce={nonce}&secret={apiSecret}&timestamp={timestamp}";
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string GenerateNonce()
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        var bytes = RandomNumberGenerator.GetBytes(16);
        var builder = new StringBuilder(16);
        foreach (var value in bytes)
        {
            builder.Append(alphabet[value % alphabet.Length]);
        }

        return builder.ToString();
    }

    private async Task<LcscSearchProductsResponse> SendSearchAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(rawJson);
            var root = document.RootElement;
            var success = root.TryGetProperty("success", out var successElement) && successElement.GetBoolean();
            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;

            var products = new List<LcscSearchProductItem>();
            if (root.TryGetProperty("result", out var resultElement) &&
                TryResolveProductCollection(resultElement, out var productArray))
            {
                foreach (var item in productArray.EnumerateArray())
                {
                    products.Add(new LcscSearchProductItem(
                        GetString(item, "productNumber", "product_number", "lcscPartNumber"),
                        GetString(item, "productCode", "product_code", "productModel", "mfrPartNumber"),
                        GetString(item, "brandNameEn", "manufacturer", "brandName"),
                        GetLong(item, "stockNumber", "stock_number", "productStock"),
                        GetDecimal(item, "lowestPrice", "lowest_price", "productPrice")));
                }
            }

            return new LcscSearchProductsResponse(success, message, rawJson, products);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "LCSC product search failed for request '{RequestUri}'.", uri);
            return new LcscSearchProductsResponse(false, exception.Message, null, []);
        }
    }

    private async Task<LcscProductInfoResponse> SendProductInfoAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(rawJson);
            var root = document.RootElement;
            var success = root.TryGetProperty("success", out var successElement) && successElement.GetBoolean();
            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;

            JsonElement? result = null;
            string? productNumber = null;
            string? manufacturerPartNumber = null;
            string? manufacturer = null;
            string? description = null;
            string? datasheetUrl = null;
            string? productImages = null;
            long? stockNumber = null;
            decimal? lowestPrice = null;

            if (root.TryGetProperty("result", out var resultElement))
            {
                result = resultElement.Clone();
                productNumber = GetString(resultElement, "productNumber", "product_number", "lcscPartNumber");
                manufacturerPartNumber = GetString(resultElement, "productCode", "product_code", "productModel", "mfrPartNumber");
                manufacturer = GetString(resultElement, "brandNameEn", "brandName", "manufacturer");
                description = GetString(resultElement, "productDescEn", "productIntroEn", "description");
                datasheetUrl = GetString(resultElement, "dataManualUrl", "productDataManualUrl", "datasheetUrl");
                productImages = GetString(resultElement, "productImages", "imageUrl", "productImageUrl");
                stockNumber = GetLong(resultElement, "stockNumber", "stock_number", "productStock");
                lowestPrice = GetDecimal(resultElement, "lowestPrice", "lowest_price", "productPrice");
            }

            return new LcscProductInfoResponse(
                success,
                message,
                rawJson,
                productNumber,
                manufacturerPartNumber,
                manufacturer,
                description,
                datasheetUrl,
                productImages,
                stockNumber,
                lowestPrice,
                result);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "LCSC product info lookup failed for request '{RequestUri}'.", uri);
            return new LcscProductInfoResponse(false, exception.Message, null, null, null, null, null, null, null, null, null, null);
        }
    }

    private Dictionary<string, string> CreateSignedParameters()
    {
        var apiKey = _options.ApiKey ?? string.Empty;
        var apiSecret = _options.ApiSecret ?? string.Empty;
        var nonce = GenerateNonce();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var signature = GenerateSignature(apiKey, nonce, apiSecret, timestamp);

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["key"] = apiKey,
            ["nonce"] = nonce,
            ["timestamp"] = timestamp,
            ["signature"] = signature
        };
    }

    private Uri BuildUri(string path, IReadOnlyDictionary<string, string> parameters)
    {
        var builder = new UriBuilder(new Uri(new Uri(_options.BaseUrl.TrimEnd('/')), path));
        builder.Query = string.Join("&", parameters.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        return builder.Uri;
    }

    private static bool TryResolveProductCollection(JsonElement resultElement, out JsonElement collection)
    {
        if (resultElement.ValueKind == JsonValueKind.Array)
        {
            collection = resultElement;
            return true;
        }

        if (resultElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var name in new[] { "list", "dataList", "productList", "items", "records" })
            {
                if (resultElement.TryGetProperty(name, out var nested) && nested.ValueKind == JsonValueKind.Array)
                {
                    collection = nested;
                    return true;
                }
            }
        }

        collection = default;
        return false;
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyIgnoreCase(element, name, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
        }

        return null;
    }

    private static long? GetLong(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetPropertyIgnoreCase(element, name, out var property))
            {
                continue;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var value))
            {
                return value;
            }

            if (property.ValueKind == JsonValueKind.String &&
                long.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static decimal? GetDecimal(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetPropertyIgnoreCase(element, name, out var property))
            {
                continue;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value))
            {
                return value;
            }

            if (property.ValueKind == JsonValueKind.String &&
                decimal.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            property = default;
            return false;
        }

        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}
