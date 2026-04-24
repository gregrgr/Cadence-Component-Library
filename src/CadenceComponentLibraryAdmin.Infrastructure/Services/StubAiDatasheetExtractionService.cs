using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class StubAiDatasheetExtractionService : IAiDatasheetExtractionService
{
    private readonly IJsonSchemaValidationService _schemaValidationService;

    public StubAiDatasheetExtractionService(IJsonSchemaValidationService schemaValidationService)
    {
        _schemaValidationService = schemaValidationService;
    }

    public async Task<AiDatasheetExtractionRunResult> RunExtractionAsync(
        AiDatasheetExtractionRunRequest request,
        CancellationToken cancellationToken = default)
    {
        var packageName = InferPackageName(request.ExistingFootprintSpecJson);
        var symbolName = $"{request.ManufacturerPartNumber}_SYM";
        var footprintName = $"{request.ManufacturerPartNumber}_FPT";

        var extractionJson = JsonSerializer.Serialize(new
        {
            manufacturer = request.Manufacturer,
            manufacturerPartNumber = request.ManufacturerPartNumber,
            summary = "Deterministic stub extraction for development and test.",
            fields = new object[]
            {
                new { path = "manufacturer", value = request.Manufacturer, confidence = 0.99m },
                new { path = "manufacturerPartNumber", value = request.ManufacturerPartNumber, confidence = 0.99m },
                new { path = "package", value = packageName, confidence = 0.90m },
                new { path = "pitch", value = 0.65m, unit = "mm", confidence = 0.82m },
                new { path = "bodySize", value = "3.0x1.7", unit = "mm", confidence = 0.80m }
            }
        }, new JsonSerializerOptions { WriteIndented = true });

        var symbolSpecJson = JsonSerializer.Serialize(new
        {
            symbolName,
            partClass = "IC",
            pinMap = new object[]
            {
                new { number = "1", name = "VCC", type = "Power" },
                new { number = "2", name = "IN", type = "Input" },
                new { number = "3", name = "OUT", type = "Output" },
                new { number = "4", name = "GND", type = "Ground" }
            }
        }, new JsonSerializerOptions { WriteIndented = true });

        var footprintSpecJson = JsonSerializer.Serialize(new
        {
            footprintName,
            packageType = packageName,
            pads = new object[]
            {
                new { name = "1", x = -0.95m, y = 0.65m, shape = "Rect", width = 0.40m, height = 0.90m },
                new { name = "2", x = -0.95m, y = 0.00m, shape = "Rect", width = 0.40m, height = 0.90m },
                new { name = "3", x = -0.95m, y = -0.65m, shape = "Rect", width = 0.40m, height = 0.90m },
                new { name = "4", x = 0.95m, y = -0.65m, shape = "Rect", width = 0.40m, height = 0.90m },
                new { name = "5", x = 0.95m, y = 0.00m, shape = "Rect", width = 0.40m, height = 0.90m },
                new { name = "6", x = 0.95m, y = 0.65m, shape = "Rect", width = 0.40m, height = 0.90m }
            }
        }, new JsonSerializerOptions { WriteIndented = true });

        var evidence = BuildEvidence(request, packageName);
        var warnings = BuildWarnings(evidence);
        var validationErrors = new List<string>();

        validationErrors.AddRange((await _schemaValidationService.ValidateAsync("component_extraction.schema.json", extractionJson, cancellationToken)).Errors);
        validationErrors.AddRange((await _schemaValidationService.ValidateAsync("symbol_spec.schema.json", symbolSpecJson, cancellationToken)).Errors);
        validationErrors.AddRange((await _schemaValidationService.ValidateAsync("footprint_spec.schema.json", footprintSpecJson, cancellationToken)).Errors);

        var status = warnings.Count == 0 && validationErrors.Count == 0
            ? AiDatasheetExtractionStatus.Draft
            : AiDatasheetExtractionStatus.NeedsReview;

        return new AiDatasheetExtractionRunResult(
            extractionJson,
            symbolSpecJson,
            footprintSpecJson,
            warnings.Count == 0 ? 0.92m : 0.71m,
            status,
            evidence,
            warnings,
            validationErrors,
            "Stub",
            null);
    }

    private static List<AiDatasheetExtractionEvidenceDraft> BuildEvidence(
        AiDatasheetExtractionRunRequest request,
        string packageName)
    {
        var hasSource = !string.IsNullOrWhiteSpace(request.SourceText) || !string.IsNullOrWhiteSpace(request.DatasheetAssetPath);
        var evidence = new List<AiDatasheetExtractionEvidenceDraft>
        {
            new("manufacturer", request.Manufacturer, null, 1, "Summary", null, 0.99m),
            new("mpn", request.ManufacturerPartNumber, null, 1, "Summary", null, 0.99m),
            new("package", packageName, null, 2, "Package", null, 0.90m),
            new("pin_table", "4-pin logical map", null, 3, "Pin table", null, 0.88m),
            new("pitch", "0.65", "mm", 4, "Package dimensions", null, 0.82m),
            new("body_size", "3.0x1.7", "mm", 4, "Package dimensions", null, 0.80m)
        };

        if (hasSource)
        {
            evidence.Add(new("pad_dimensions", "0.40x0.90", "mm", 5, "Land pattern", null, 0.77m));
            evidence.Add(new("pin1_orientation", "Top-left chamfer", null, 6, null, "Pin 1 marker", 0.75m));
        }

        return evidence;
    }

    private static List<string> BuildWarnings(IReadOnlyCollection<AiDatasheetExtractionEvidenceDraft> evidence)
    {
        var warnings = new List<string>();
        var criticalFields = new[]
        {
            "manufacturer",
            "mpn",
            "package",
            "pin_table",
            "pad_dimensions",
            "pitch",
            "body_size",
            "pin1_orientation"
        };

        foreach (var field in criticalFields)
        {
            if (!evidence.Any(x => string.Equals(x.FieldPath, field, StringComparison.OrdinalIgnoreCase)))
            {
                warnings.Add($"Missing evidence for critical field '{field}'.");
            }
        }

        return warnings;
    }

    private static string InferPackageName(string? footprintSpecJson)
    {
        if (string.IsNullOrWhiteSpace(footprintSpecJson))
        {
            return "UNKNOWN_PACKAGE";
        }

        try
        {
            using var document = JsonDocument.Parse(footprintSpecJson);
            if (document.RootElement.TryGetProperty("packageType", out var packageType) && packageType.ValueKind == JsonValueKind.String)
            {
                return packageType.GetString() ?? "UNKNOWN_PACKAGE";
            }
        }
        catch (JsonException)
        {
        }

        return "UNKNOWN_PACKAGE";
    }
}
