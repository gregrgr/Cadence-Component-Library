using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface ICadenceVerificationReportService
{
    Task<VerificationReportResult> GenerateDevelopmentReportAsync(
        long extractionId,
        string actor,
        CancellationToken cancellationToken = default);
}
