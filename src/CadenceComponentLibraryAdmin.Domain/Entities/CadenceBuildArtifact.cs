using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class CadenceBuildArtifact
{
    public long Id { get; set; }
    public long CadenceBuildJobId { get; set; }
    public CadenceBuildJob CadenceBuildJob { get; set; } = null!;
    public CadenceBuildArtifactType ArtifactType { get; set; }
    public string FilePath { get; set; } = null!;
    public string? Sha256 { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
