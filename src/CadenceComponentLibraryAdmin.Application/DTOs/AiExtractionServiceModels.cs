using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed record DatasheetTextExtractionRequest(
    long ExtractionId,
    string? DatasheetAssetPath);

public sealed record DatasheetTextExtractionResult(
    string ExtractedText,
    IReadOnlyList<string> Warnings);

public sealed record AiDatasheetExtractionEvidenceDraft(
    string FieldPath,
    string ValueText,
    string? Unit,
    int? SourcePage,
    string? SourceTable,
    string? SourceFigure,
    decimal Confidence,
    AiExtractionReviewerDecision ReviewerDecision = AiExtractionReviewerDecision.Pending,
    string? ReviewerNote = null);

public sealed record AiDatasheetExtractionRunRequest(
    long ExtractionId,
    string Manufacturer,
    string ManufacturerPartNumber,
    string? DatasheetAssetPath,
    string? SourceText,
    string? ExistingExtractionJson,
    string? ExistingSymbolSpecJson,
    string? ExistingFootprintSpecJson);

public sealed record AiDatasheetExtractionRunResult(
    string ExtractionJson,
    string SymbolSpecJson,
    string FootprintSpecJson,
    decimal Confidence,
    AiDatasheetExtractionStatus Status,
    IReadOnlyList<AiDatasheetExtractionEvidenceDraft> Evidence,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> ValidationErrors,
    string? ProviderName,
    string? RawModelOutput);

public sealed record JsonSchemaValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);
