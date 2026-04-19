using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IQualityReportService
{
    Task<QualityReportSummary> BuildSummaryAsync(CancellationToken cancellationToken = default);
}
