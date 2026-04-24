using System.Text.Json;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Services;

namespace CadenceComponentLibraryAdmin.CadenceBridge.Queue;

public sealed class AllegroFootprintJobBuilder
{
    public const string AllowedAction = "create_footprint";
    public const string DefaultOverwritePolicy = "fail_if_exists";

    public string Build(
        CadenceBuildJob job,
        AiDatasheetExtraction extraction,
        CadenceAutomationOptions options,
        string? action = null,
        string? overwritePolicy = null)
    {
        var normalizedAction = Normalize(action) ?? AllowedAction;
        if (!string.Equals(normalizedAction, AllowedAction, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unsupported Allegro action '{normalizedAction}'.");
        }

        var normalizedPolicy = Normalize(overwritePolicy) ?? DefaultOverwritePolicy;
        var document = new CadenceQueueJobDocument
        {
            JobId = job.Id,
            QueueFamily = "allegro",
            Action = normalizedAction,
            OverwritePolicy = normalizedPolicy,
            CandidateId = job.CandidateId,
            AiDatasheetExtractionId = job.AiDatasheetExtractionId,
            Manufacturer = extraction.Manufacturer,
            ManufacturerPartNumber = extraction.ManufacturerPartNumber,
            LibraryRoot = options.LibraryRoot,
            SpecJson = extraction.FootprintSpecJson,
            ResultJsonPath = Path.Combine(options.AllegroQueuePath, "done", $"{job.Id}.result.json").Replace('\\', '/'),
            RequestedByTool = job.ToolName,
            RequestedAtUtc = job.CreatedAtUtc.ToString("O")
        };

        return JsonSerializer.Serialize(document, JsonSerializerOptions.Web);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
