using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IMcpLibraryWorkflowService
{
    Task<LibraryCandidateSummaryResult> GetCandidateAsync(
        LibraryGetCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task<LibraryDuplicateSearchResult> SearchDuplicateAsync(
        LibrarySearchDuplicateRequest request,
        CancellationToken cancellationToken = default);

    Task<DatasheetExtractionResult> CreateExtractionDraftAsync(
        DatasheetCreateExtractionDraftRequest request,
        CancellationToken cancellationToken = default);

    Task<DatasheetExtractionResult> SubmitForReviewAsync(
        long extractionId,
        CancellationToken cancellationToken = default);

    Task<DatasheetExtractionResult> ApproveForBuildAsync(
        long extractionId,
        CancellationToken cancellationToken = default);

    Task<CadenceJobStatusResult> EnqueueCaptureSymbolJobAsync(
        CadenceEnqueueJobRequest request,
        CancellationToken cancellationToken = default);

    Task<CadenceJobStatusResult> EnqueueAllegroFootprintJobAsync(
        CadenceEnqueueJobRequest request,
        CancellationToken cancellationToken = default);

    Task<CadenceJobStatusResult> GetJobStatusAsync(
        long jobId,
        CancellationToken cancellationToken = default);

    Task<VerificationReportResult?> GetVerificationReportAsync(
        VerificationGetReportRequest request,
        CancellationToken cancellationToken = default);
}
