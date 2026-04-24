using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.CadenceBridge.Queue;

public sealed record CadenceQueueArtifactInput(
    CadenceBuildArtifactType ArtifactType,
    string FilePath);
