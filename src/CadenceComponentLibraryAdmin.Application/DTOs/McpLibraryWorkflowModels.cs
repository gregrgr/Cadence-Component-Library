using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed record LibraryGetCandidateRequest(long? CandidateId, long? ExternalImportId);

public sealed record LibraryCandidateSummaryResult(
    long? CandidateId,
    long? ExternalImportId,
    string? SourceProvider,
    string? Manufacturer,
    string? ManufacturerPartNumber,
    string? Description,
    string? RawPackageName,
    CandidateStatus? CandidateStatus,
    IReadOnlyList<AiExtractionStatusSummary> Extractions,
    IReadOnlyList<CadenceJobStatusSummary> BuildJobs);

public sealed record AiExtractionStatusSummary(
    long ExtractionId,
    AiDatasheetExtractionStatus Status,
    decimal Confidence,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc);

public sealed record CadenceJobStatusSummary(
    long JobId,
    CadenceBuildJobType JobType,
    CadenceBuildJobStatus Status,
    DateTime CreatedAtUtc,
    DateTime? FinishedAtUtc);

public sealed record LibrarySearchDuplicateRequest(
    string Manufacturer,
    string ManufacturerPartNumber,
    string? PackageName);

public sealed record DuplicateMatchSummary(
    long Id,
    string DisplayName,
    string? Description,
    string? MatchReason);

public sealed record LibraryDuplicateSearchResult(
    IReadOnlyList<DuplicateMatchSummary> CompanyParts,
    IReadOnlyList<DuplicateMatchSummary> ManufacturerParts,
    IReadOnlyList<DuplicateMatchSummary> PackageFamilies,
    IReadOnlyList<DuplicateMatchSummary> FootprintVariants);

public sealed record DatasheetCreateExtractionDraftRequest(
    long? CandidateId,
    long? ExternalImportId,
    string? DatasheetAssetPath,
    string ExtractionJson,
    string SymbolSpecJson,
    string FootprintSpecJson);

public sealed record DatasheetExtractionResult(
    long ExtractionId,
    AiDatasheetExtractionStatus Status);

public sealed record CadenceEnqueueJobRequest(long ExtractionId);

public sealed record CadenceJobStatusResult(
    long JobId,
    CadenceBuildJobType JobType,
    CadenceBuildJobStatus Status,
    string ToolName,
    string? ToolVersion,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? FinishedAtUtc,
    string? ErrorMessage,
    IReadOnlyList<CadenceArtifactSummary> Artifacts);

public sealed record CadenceArtifactSummary(
    long ArtifactId,
    CadenceBuildArtifactType ArtifactType,
    string FilePath,
    string? Sha256,
    DateTime CreatedAtUtc);

public sealed record VerificationGetReportRequest(long? ExtractionId, long? JobId);

public sealed record VerificationReportResult(
    long ReportId,
    long? CandidateId,
    long? CompanyPartId,
    long? AiDatasheetExtractionId,
    LibraryVerificationOverallStatus OverallStatus,
    DateTime CreatedAtUtc,
    string? SymbolReportJson,
    string? FootprintReportJson);
