using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class OpenAiCompatibleDatasheetExtractionService : IAiDatasheetExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly IJsonSchemaValidationService _schemaValidationService;
    private readonly OpenAiCompatibleOptions _options;
    private readonly ILogger<OpenAiCompatibleDatasheetExtractionService> _logger;

    public OpenAiCompatibleDatasheetExtractionService(
        HttpClient httpClient,
        IJsonSchemaValidationService schemaValidationService,
        IOptions<AiExtractionOptions> options,
        ILogger<OpenAiCompatibleDatasheetExtractionService> logger)
    {
        _httpClient = httpClient;
        _schemaValidationService = schemaValidationService;
        _options = options.Value.OpenAI;
        _logger = logger;
    }

    public async Task<AiDatasheetExtractionRunResult> RunExtractionAsync(
        AiDatasheetExtractionRunRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("OpenAI-compatible extraction is disabled; falling back to explicit operator action.");
            throw new InvalidOperationException("OpenAI-compatible extraction is disabled.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("OpenAI-compatible extraction is enabled but no API key is configured.");
            throw new InvalidOperationException("OpenAI-compatible extraction API key is not configured.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/responses");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            model = _options.Model,
            input = $"Extract component, symbol, and footprint JSON for {request.Manufacturer} {request.ManufacturerPartNumber}. Datasheet text: {request.SourceText ?? string.Empty}"
        };

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var rawOutput = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI-compatible extraction failed with HTTP {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException($"OpenAI-compatible extraction failed with HTTP {(int)response.StatusCode}.");
        }

        var validationErrors = new List<string>();
        validationErrors.AddRange((await _schemaValidationService.ValidateAsync("component_extraction.schema.json", "{}", cancellationToken)).Errors);

        return new AiDatasheetExtractionRunResult(
            "{}",
            "{}",
            "{}",
            0m,
            AiDatasheetExtractionStatus.NeedsReview,
            [],
            ["OpenAI-compatible implementation is not finalized; review is mandatory."],
            validationErrors,
            "OpenAICompatible",
            rawOutput);
    }
}
