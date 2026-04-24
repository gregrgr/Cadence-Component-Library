using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class AiDatasheetExtraction
{
    public long Id { get; set; }
    public long? CandidateId { get; set; }
    public OnlineCandidate? Candidate { get; set; }
    public long? ExternalImportId { get; set; }
    public ExternalComponentImport? ExternalImport { get; set; }
    public string Manufacturer { get; set; } = null!;
    public string ManufacturerPartNumber { get; set; } = null!;
    public string? DatasheetAssetPath { get; set; }
    public string ExtractionJson { get; set; } = null!;
    public string SymbolSpecJson { get; set; } = null!;
    public string FootprintSpecJson { get; set; } = null!;
    public decimal Confidence { get; set; }
    public AiDatasheetExtractionStatus Status { get; set; } = AiDatasheetExtractionStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    public ICollection<AiExtractionEvidence> EvidenceItems { get; set; } = new List<AiExtractionEvidence>();
    public ICollection<CadenceBuildJob> BuildJobs { get; set; } = new List<CadenceBuildJob>();
    public ICollection<LibraryVerificationReport> VerificationReports { get; set; } = new List<LibraryVerificationReport>();
}
