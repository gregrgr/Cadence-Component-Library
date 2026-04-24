using System.Net;
using System.Text;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class AiDatasheetExtractionServiceTests
{
    [Fact]
    public async Task StubExtraction_ProducesValidJson()
    {
        var validator = new JsonSchemaValidationService();
        var service = new StubAiDatasheetExtractionService(validator);

        var result = await service.RunExtractionAsync(
            new AiDatasheetExtractionRunRequest(
                1,
                "Texas Instruments",
                "SN74LVC1G14DBVR",
                "datasheets/ti.pdf",
                "Package dimensions and pin table present.",
                "{}",
                "{}",
                "{\"packageType\":\"SOT-23-5\"}"));

        Assert.Empty(result.ValidationErrors);
        Assert.NotEmpty(result.ExtractionJson);
        Assert.NotEmpty(result.SymbolSpecJson);
        Assert.NotEmpty(result.FootprintSpecJson);
    }

    [Fact]
    public async Task MissingEvidence_ProducesWarning()
    {
        var validator = new JsonSchemaValidationService();
        var service = new StubAiDatasheetExtractionService(validator);

        var result = await service.RunExtractionAsync(
            new AiDatasheetExtractionRunRequest(
                1,
                "Analog Devices",
                "ADP150AUJZ",
                null,
                string.Empty,
                "{}",
                "{}",
                "{\"packageType\":\"TSOT-23\"}"));

        Assert.Contains(result.Warnings, x => x.Contains("pad_dimensions", StringComparison.Ordinal));
        Assert.Equal(Domain.Enums.AiDatasheetExtractionStatus.NeedsReview, result.Status);
    }

    [Fact]
    public async Task InvalidJson_Rejected()
    {
        var validator = new JsonSchemaValidationService();

        var result = await validator.ValidateAsync("component_extraction.schema.json", "{ invalid");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ApiKey_NeverAppearsInLogs()
    {
        const string apiKey = "super-secret-api-key";
        var logger = new TestLogger<OpenAiCompatibleDatasheetExtractionService>();
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("boom", Encoding.UTF8, "text/plain")
            });

        var service = new OpenAiCompatibleDatasheetExtractionService(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.example.local") },
            new JsonSchemaValidationService(),
            Options.Create(new AiExtractionOptions
            {
                Mode = "OpenAI",
                OpenAI = new OpenAiCompatibleOptions
                {
                    Enabled = true,
                    BaseUrl = "https://api.example.local",
                    Model = "gpt-test",
                    ApiKey = apiKey
                }
            }),
            logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunExtractionAsync(
            new AiDatasheetExtractionRunRequest(1, "ACME", "PART-1", null, "text", "{}", "{}", "{}")));

        Assert.DoesNotContain(logger.Messages, x => x.Contains(apiKey, StringComparison.Ordinal));
    }

    [Fact]
    public async Task CodexCliExtraction_ParsesValidStructuredOutput()
    {
        var service = new CodexCliDatasheetExtractionService(
            new FakeCodexCliRunner("""
            {
              "componentExtraction": {
                "manufacturer": "Texas Instruments",
                "manufacturerPartNumber": "SN74LVC1G14DBVR",
                "fields": [
                  { "path": "package", "value": "SOT-23-5", "confidence": 0.91 }
                ]
              },
              "symbolSpec": {
                "symbolName": "SN74LVC1G14DBVR_SYM",
                "pinMap": [
                  { "number": "1", "name": "A", "type": "Input" }
                ]
              },
              "footprintSpec": {
                "footprintName": "SOT-23-5_TI",
                "pads": [
                  { "name": "1", "x": 0, "y": 0, "width": 0.4, "height": 0.9 }
                ]
              },
              "confidence": 0.87,
              "evidence": [
                { "fieldPath": "manufacturer", "valueText": "Texas Instruments", "confidence": 0.99 },
                { "fieldPath": "mpn", "valueText": "SN74LVC1G14DBVR", "confidence": 0.99 },
                { "fieldPath": "package", "valueText": "SOT-23-5", "confidence": 0.91 },
                { "fieldPath": "pin_table", "valueText": "Pin table", "confidence": 0.88 },
                { "fieldPath": "pad_dimensions", "valueText": "0.4x0.9", "unit": "mm", "confidence": 0.82 },
                { "fieldPath": "pitch", "valueText": "0.65", "unit": "mm", "confidence": 0.82 },
                { "fieldPath": "body_size", "valueText": "3.0x1.7", "unit": "mm", "confidence": 0.80 },
                { "fieldPath": "pin1_orientation", "valueText": "top-left marker", "confidence": 0.75 }
              ],
              "warnings": []
            }
            """),
            new JsonSchemaValidationService(),
            NullLogger<CodexCliDatasheetExtractionService>.Instance);

        var result = await service.RunExtractionAsync(
            new AiDatasheetExtractionRunRequest(
                1,
                "Texas Instruments",
                "SN74LVC1G14DBVR",
                null,
                "datasheet text",
                "{}",
                "{}",
                "{}"));

        Assert.Equal("CodexCli", result.ProviderName);
        Assert.Empty(result.ValidationErrors);
        Assert.Empty(result.Warnings);
        Assert.Equal(Domain.Enums.AiDatasheetExtractionStatus.Draft, result.Status);
        Assert.Contains("SN74LVC1G14DBVR_SYM", result.SymbolSpecJson, StringComparison.Ordinal);
        Assert.Equal(8, result.Evidence.Count);
    }

    [Fact]
    public async Task CodexCliExtraction_InvalidOutputIsRejectedWithoutRunningRealCli()
    {
        var service = new CodexCliDatasheetExtractionService(
            new FakeCodexCliRunner("not json"),
            new JsonSchemaValidationService(),
            NullLogger<CodexCliDatasheetExtractionService>.Instance);

        var result = await service.RunExtractionAsync(
            new AiDatasheetExtractionRunRequest(
                1,
                "ACME",
                "PART-1",
                null,
                "datasheet text",
                "{\"manufacturer\":\"ACME\",\"manufacturerPartNumber\":\"PART-1\",\"fields\":[]}",
                "{\"symbolName\":\"PART-1\",\"pinMap\":[]}",
                "{\"footprintName\":\"PART-1\",\"pads\":[]}"));

        Assert.Equal("CodexCli", result.ProviderName);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Equal(Domain.Enums.AiDatasheetExtractionStatus.NeedsReview, result.Status);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    private sealed class FakeCodexCliRunner(string output) : ICodexCliRunner
    {
        public Task<CodexCliRunResult> RunAsync(
            CodexCliRunRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new CodexCliRunResult(0, output, string.Empty));
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
