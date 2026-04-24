using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.CadenceBridge.Queue;

public interface ICadenceJobQueue
{
    Task EnqueueAsync(CadenceBuildJob job, CancellationToken cancellationToken = default);
    Task<CadenceBuildJob> GetJobAsync(long jobId, CancellationToken cancellationToken = default);
    Task MarkRunningAsync(long jobId, CancellationToken cancellationToken = default);
    Task MarkSucceededAsync(
        long jobId,
        string outputJson,
        IReadOnlyCollection<CadenceQueueArtifactInput> artifacts,
        CancellationToken cancellationToken = default);
    Task MarkFailedAsync(long jobId, string error, CancellationToken cancellationToken = default);
}
