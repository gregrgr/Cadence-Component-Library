using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class CadenceBuildJob
{
    public long Id { get; set; }
    public CadenceBuildJobType JobType { get; set; }
    public long? CandidateId { get; set; }
    public OnlineCandidate? Candidate { get; set; }
    public long? AiDatasheetExtractionId { get; set; }
    public AiDatasheetExtraction? AiDatasheetExtraction { get; set; }
    public string InputJson { get; set; } = null!;
    public string? OutputJson { get; set; }
    public CadenceBuildJobStatus Status { get; set; } = CadenceBuildJobStatus.Pending;
    public string ToolName { get; set; } = null!;
    public string? ToolVersion { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<CadenceBuildArtifact> Artifacts { get; set; } = new List<CadenceBuildArtifact>();
}
