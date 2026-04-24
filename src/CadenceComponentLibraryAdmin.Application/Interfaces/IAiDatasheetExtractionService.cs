using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IAiDatasheetExtractionService
{
    Task<AiDatasheetExtractionRunResult> RunExtractionAsync(
        AiDatasheetExtractionRunRequest request,
        CancellationToken cancellationToken = default);
}
