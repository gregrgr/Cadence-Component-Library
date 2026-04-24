using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class JsonSchemaValidationService : IJsonSchemaValidationService
{
    public Task<JsonSchemaValidationResult> ValidateAsync(
        string schemaFileName,
        string json,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        JsonDocument? document = null;

        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            return Task.FromResult(new JsonSchemaValidationResult(false, [$"Invalid JSON: {ex.Message}"]));
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                errors.Add("JSON root must be an object.");
            }

            switch (schemaFileName)
            {
                case "component_extraction.schema.json":
                    RequireString(root, "manufacturer", errors);
                    RequireString(root, "manufacturerPartNumber", errors);
                    RequireArray(root, "fields", errors);
                    break;
                case "symbol_spec.schema.json":
                    RequireString(root, "symbolName", errors);
                    RequireArray(root, "pinMap", errors);
                    break;
                case "footprint_spec.schema.json":
                    RequireString(root, "footprintName", errors);
                    RequireArray(root, "pads", errors);
                    break;
                case "cadence_build_result.schema.json":
                    RequireString(root, "jobType", errors);
                    RequireString(root, "status", errors);
                    break;
                case "verification_report.schema.json":
                    RequireString(root, "overallStatus", errors);
                    break;
                default:
                    errors.Add($"Unsupported schema '{schemaFileName}'.");
                    break;
            }
        }

        return Task.FromResult(new JsonSchemaValidationResult(errors.Count == 0, errors));
    }

    private static void RequireString(JsonElement root, string propertyName, List<string> errors)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.GetString()))
        {
            errors.Add($"Property '{propertyName}' is required and must be a non-empty string.");
        }
    }

    private static void RequireArray(JsonElement root, string propertyName, List<string> errors)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            errors.Add($"Property '{propertyName}' is required and must be an array.");
        }
    }
}
