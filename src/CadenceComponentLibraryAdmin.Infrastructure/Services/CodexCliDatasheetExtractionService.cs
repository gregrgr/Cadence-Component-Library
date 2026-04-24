using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class CodexCliDatasheetExtractionService : IAiDatasheetExtractionService
{
    private static readonly string[] CriticalEvidenceFields =
    [
        "manufacturer",
        "mpn",
        "package",
        "pin_table",
        "pad_dimensions",
        "pitch",
        "body_size",
        "pin1_orientation"
    ];

    private readonly ICodexCliRunner _codexCliRunner;
    private readonly IJsonSchemaValidationService _schemaValidationService;
    private readonly ILogger<CodexCliDatasheetExtractionService> _logger;

    public CodexCliDatasheetExtractionService(
        ICodexCliRunner codexCliRunner,
        IJsonSchemaValidationService schemaValidationService,
        ILogger<CodexCliDatasheetExtractionService> logger)
    {
        _codexCliRunner = codexCliRunner;
        _schemaValidationService = schemaValidationService;
        _logger = logger;
    }

    public async Task<AiDatasheetExtractionRunResult> RunExtractionAsync(
        AiDatasheetExtractionRunRequest request,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(request);
        var cliResult = await _codexCliRunner.RunAsync(new CodexCliRunRequest(prompt), cancellationToken);
        if (cliResult.ExitCode != 0)
        {
            throw new InvalidOperationException($"Codex CLI extraction failed with exit code {cliResult.ExitCode}.");
        }

        var rawOutput = cliResult.Output;
        var json = ExtractJsonObject(rawOutput);
        if (json is null)
        {
            _logger.LogWarning("Codex CLI extraction did not return a JSON object.");
            return FailureResult(request, rawOutput, "Codex CLI output did not contain a JSON object.");
        }

        CodexCliExtractionEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<CodexCliExtractionEnvelope>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            return FailureResult(request, rawOutput, $"Codex CLI output JSON could not be parsed: {ex.Message}");
        }

        if (envelope is null)
        {
            return FailureResult(request, rawOutput, "Codex CLI output JSON was empty.");
        }

        var extractionJson = ToJson(envelope.ComponentExtraction);
        var symbolSpecJson = ToJson(envelope.SymbolSpec);
        var footprintSpecJson = ToJson(envelope.FootprintSpec);

        var evidence = envelope.Evidence
            .Select(x => new AiDatasheetExtractionEvidenceDraft(
                x.FieldPath ?? string.Empty,
                x.ValueText ?? string.Empty,
                x.Unit,
                x.SourcePage,
                x.SourceTable,
                x.SourceFigure,
                x.Confidence ?? 0m,
                ParseReviewerDecision(x.ReviewerDecision),
                x.ReviewerNote))
            .Where(x => !string.IsNullOrWhiteSpace(x.FieldPath) && !string.IsNullOrWhiteSpace(x.ValueText))
            .ToList();

        var validationErrors = new List<string>();
        validationErrors.AddRange((await _schemaValidationService.ValidateAsync("component_extraction.schema.json", extractionJson, cancellationToken)).Errors);
        validationErrors.AddRange((await _schemaValidationService.ValidateAsync("symbol_spec.schema.json", symbolSpecJson, cancellationToken)).Errors);
        validationErrors.AddRange((await _schemaValidationService.ValidateAsync("footprint_spec.schema.json", footprintSpecJson, cancellationToken)).Errors);

        var warnings = new List<string>();
        warnings.AddRange(envelope.Warnings.Where(x => !string.IsNullOrWhiteSpace(x)));
        warnings.AddRange(BuildEvidenceWarnings(evidence));

        var status = warnings.Count == 0 && validationErrors.Count == 0
            ? AiDatasheetExtractionStatus.Draft
            : AiDatasheetExtractionStatus.NeedsReview;

        return new AiDatasheetExtractionRunResult(
            extractionJson,
            symbolSpecJson,
            footprintSpecJson,
            envelope.Confidence ?? 0m,
            status,
            evidence,
            warnings,
            validationErrors,
            "CodexCli",
            rawOutput);
    }

    private static string BuildPrompt(AiDatasheetExtractionRunRequest request)
    {
        return $$"""
You are extracting a datasheet into reviewable JSON for CadenceComponentLibraryAdmin.

Return only one JSON object. Do not use Markdown.

The JSON object must have this shape:
{
  "componentExtraction": {
    "manufacturer": "string",
    "manufacturerPartNumber": "string",
    "fields": []
  },
  "symbolSpec": {
    "symbolName": "string",
    "pinMap": []
  },
  "footprintSpec": {
    "footprintName": "string",
    "pads": []
  },
  "confidence": 0.0,
  "evidence": [
    {
      "fieldPath": "manufacturer | mpn | package | pin_table | pad_dimensions | pitch | body_size | pin1_orientation",
      "valueText": "string",
      "unit": null,
      "sourcePage": null,
      "sourceTable": null,
      "sourceFigure": null,
      "confidence": 0.0,
      "reviewerDecision": "Pending",
      "reviewerNote": null
    }
  ],
  "warnings": []
}

Rules:
- If a value is uncertain, keep it in componentExtraction.fields with low confidence and add a warning.
- Every critical field should have evidence: manufacturer, mpn, package, pin_table, pad_dimensions, pitch, body_size, pin1_orientation.
- Do not invent Cadence file paths or approved library artifacts.
- Do not output Tcl, SKILL, shell commands, or instructions to execute tools.

Known metadata:
- extractionId: {{request.ExtractionId}}
- manufacturer: {{request.Manufacturer}}
- manufacturerPartNumber: {{request.ManufacturerPartNumber}}
- datasheetAssetPath: {{request.DatasheetAssetPath}}

Existing component extraction JSON:
{{request.ExistingExtractionJson}}

Existing symbol spec JSON:
{{request.ExistingSymbolSpecJson}}

Existing footprint spec JSON:
{{request.ExistingFootprintSpecJson}}

Datasheet text:
{{request.SourceText}}
""";
    }

    private static AiDatasheetExtractionRunResult FailureResult(
        AiDatasheetExtractionRunRequest request,
        string rawOutput,
        string error)
    {
        return new AiDatasheetExtractionRunResult(
            request.ExistingExtractionJson ?? "{}",
            request.ExistingSymbolSpecJson ?? "{}",
            request.ExistingFootprintSpecJson ?? "{}",
            0m,
            AiDatasheetExtractionStatus.NeedsReview,
            [],
            [],
            [error],
            "CodexCli",
            rawOutput);
    }

    private static IReadOnlyList<string> BuildEvidenceWarnings(IReadOnlyCollection<AiDatasheetExtractionEvidenceDraft> evidence)
    {
        var warnings = new List<string>();
        foreach (var field in CriticalEvidenceFields)
        {
            if (!evidence.Any(x => string.Equals(x.FieldPath, field, StringComparison.OrdinalIgnoreCase)))
            {
                warnings.Add($"Missing evidence for critical field '{field}'.");
            }
        }

        return warnings;
    }

    private static AiExtractionReviewerDecision ParseReviewerDecision(string? value)
    {
        return Enum.TryParse<AiExtractionReviewerDecision>(value, ignoreCase: true, out var decision)
            ? decision
            : AiExtractionReviewerDecision.Pending;
    }

    private static string ToJson(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null
            ? "{}"
            : JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string? ExtractJsonObject(string output)
    {
        var start = output.IndexOf('{');
        if (start < 0)
        {
            return null;
        }

        var depth = 0;
        var inString = false;
        var escaped = false;

        for (var i = start; i < output.Length; i++)
        {
            var current = output[i];
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (current == '\\' && inString)
            {
                escaped = true;
                continue;
            }

            if (current == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString)
            {
                continue;
            }

            if (current == '{')
            {
                depth++;
            }
            else if (current == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return output[start..(i + 1)];
                }
            }
        }

        return null;
    }

    private sealed class CodexCliExtractionEnvelope
    {
        public JsonElement ComponentExtraction { get; set; }
        public JsonElement SymbolSpec { get; set; }
        public JsonElement FootprintSpec { get; set; }
        public decimal? Confidence { get; set; }
        public List<CodexCliEvidenceItem> Evidence { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
    }

    private sealed class CodexCliEvidenceItem
    {
        public string? FieldPath { get; set; }
        public string? ValueText { get; set; }
        public string? Unit { get; set; }
        public int? SourcePage { get; set; }
        public string? SourceTable { get; set; }
        public string? SourceFigure { get; set; }
        public decimal? Confidence { get; set; }
        public string? ReviewerDecision { get; set; }
        public string? ReviewerNote { get; set; }
    }
}
