using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface ICadenceBuildJobQueue
{
    Task EnqueueAsync(CadenceBuildJob job, CancellationToken cancellationToken = default);
}
