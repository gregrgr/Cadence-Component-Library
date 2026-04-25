using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.Cadence;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Services;

namespace CadenceComponentLibraryAdmin.CadenceBridge.Queue;

public sealed class CaptureSymbolJobBuilder
{
    public const string AllowedAction = CadenceQueueActions.CreateSymbol;
    public const string DefaultOverwritePolicy = CadenceQueueActions.FailIfExists;

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
            throw new InvalidOperationException($"Unsupported Capture action '{normalizedAction}'.");
        }

        var normalizedPolicy = Normalize(overwritePolicy) ?? DefaultOverwritePolicy;
        var document = new CadenceQueueJobDocument
        {
            JobId = job.Id,
            QueueFamily = "capture",
            Action = normalizedAction,
            OverwritePolicy = normalizedPolicy,
            CandidateId = job.CandidateId,
            AiDatasheetExtractionId = job.AiDatasheetExtractionId,
            Manufacturer = extraction.Manufacturer,
            ManufacturerPartNumber = extraction.ManufacturerPartNumber,
            LibraryRoot = options.LibraryRoot,
            SpecJson = extraction.SymbolSpecJson,
            ResultJsonPath = Path.Combine(options.CaptureQueuePath, "done", $"{job.Id}.result.json").Replace('\\', '/'),
            RequestedByTool = job.ToolName,
            RequestedAtUtc = job.CreatedAtUtc.ToString("O")
        };

        return JsonSerializer.Serialize(document, JsonSerializerOptions.Web);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
