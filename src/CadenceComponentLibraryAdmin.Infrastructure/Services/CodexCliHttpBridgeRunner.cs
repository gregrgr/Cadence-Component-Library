using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class CodexCliHttpBridgeRunner : ICodexCliRunner
{
    private readonly HttpClient _httpClient;
    private readonly CodexCliOptions _options;
    private readonly ILogger<CodexCliHttpBridgeRunner> _logger;

    public CodexCliHttpBridgeRunner(
        HttpClient httpClient,
        IOptions<AiExtractionOptions> options,
        ILogger<CodexCliHttpBridgeRunner> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.CodexCli;
        _logger = logger;
    }

    public async Task<CodexCliRunResult> RunAsync(
        CodexCliRunRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BridgeUrl))
        {
            throw new InvalidOperationException("Codex CLI bridge URL is not configured.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(new Uri(_options.BridgeUrl.TrimEnd('/') + "/"), "extract"));

        if (!string.IsNullOrWhiteSpace(_options.BridgeToken))
        {
            httpRequest.Headers.Add("X-Codex-Bridge-Token", _options.BridgeToken);
        }

        httpRequest.Content = JsonContent.Create(new
        {
            request.Prompt,
            _options.Command,
            _options.Model,
            _options.Profile,
            _options.Sandbox,
            _options.Ephemeral,
            _options.TimeoutSeconds,
            _options.WorkingDirectory
        });

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Codex CLI bridge returned HTTP {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException($"Codex CLI bridge returned HTTP {(int)response.StatusCode}: {ReadError(rawBody)}");
        }

        try
        {
            var result = JsonSerializer.Deserialize<CodexCliBridgeResponse>(
                rawBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result is null
                ? throw new InvalidOperationException("Codex CLI bridge returned an empty response.")
                : new CodexCliRunResult(result.ExitCode, result.Output ?? string.Empty, result.ErrorOutput ?? string.Empty);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Codex CLI bridge response was not valid JSON: {ex.Message}", ex);
        }
    }

    private sealed class CodexCliBridgeResponse
    {
        public int ExitCode { get; set; }
        public string? Output { get; set; }
        public string? ErrorOutput { get; set; }
    }

    private static string ReadError(string rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return "No response body.";
        }

        try
        {
            using var document = JsonDocument.Parse(rawBody);
            if (document.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            {
                return error.GetString() ?? "Unknown error.";
            }
        }
        catch (JsonException)
        {
        }

        return rawBody.Length > 500 ? rawBody[..500] : rawBody;
    }
}
