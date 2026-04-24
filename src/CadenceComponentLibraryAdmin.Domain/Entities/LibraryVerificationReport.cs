using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class LibraryVerificationReport
{
    public long Id { get; set; }
    public long? CandidateId { get; set; }
    public OnlineCandidate? Candidate { get; set; }
    public long? CompanyPartId { get; set; }
    public CompanyPart? CompanyPart { get; set; }
    public long? AiDatasheetExtractionId { get; set; }
    public AiDatasheetExtraction? AiDatasheetExtraction { get; set; }
    public string? SymbolReportJson { get; set; }
    public string? FootprintReportJson { get; set; }
    public LibraryVerificationOverallStatus OverallStatus { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
