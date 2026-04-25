using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface ICadenceJobSimulator
{
    Task<CadenceJobStatusResult> SimulateSuccessAsync(
        long jobId,
        string actor,
        CancellationToken cancellationToken = default);
}
