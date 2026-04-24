using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;

namespace CadenceComponentLibraryAdmin.Mcp.Mcp;

public sealed class LibraryMcpToolCatalog
{
    private readonly IMcpLibraryWorkflowService _workflowService;

    public LibraryMcpToolCatalog(IMcpLibraryWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public Task<LibraryCandidateSummaryResult> LibraryGetCandidateAsync(
        long? candidateId,
        long? externalImportId,
        CancellationToken cancellationToken = default)
        => _workflowService.GetCandidateAsync(new LibraryGetCandidateRequest(candidateId, externalImportId), cancellationToken);

    public Task<LibraryDuplicateSearchResult> LibrarySearchDuplicateAsync(
        string manufacturer,
        string manufacturerPartNumber,
        string? packageName,
        CancellationToken cancellationToken = default)
        => _workflowService.SearchDuplicateAsync(
            new LibrarySearchDuplicateRequest(manufacturer, manufacturerPartNumber, packageName),
            cancellationToken);

    public Task<DatasheetExtractionResult> DatasheetCreateExtractionDraftAsync(
        long? candidateId,
        long? externalImportId,
        string extractionJson,
        string symbolSpecJson,
        string footprintSpecJson,
        CancellationToken cancellationToken = default)
        => _workflowService.CreateExtractionDraftAsync(
            new DatasheetCreateExtractionDraftRequest(
                candidateId,
                externalImportId,
                null,
                extractionJson,
                symbolSpecJson,
                footprintSpecJson),
            cancellationToken);

    public Task<DatasheetExtractionResult> DatasheetSubmitForReviewAsync(
        long extractionId,
        CancellationToken cancellationToken = default)
        => _workflowService.SubmitForReviewAsync(extractionId, cancellationToken);

    public Task<DatasheetExtractionResult> DatasheetApproveForBuildAsync(
        long extractionId,
        CancellationToken cancellationToken = default)
        => _workflowService.ApproveForBuildAsync(extractionId, cancellationToken);

    public Task<CadenceJobStatusResult> CaptureEnqueueSymbolJobAsync(
        long extractionId,
        CancellationToken cancellationToken = default)
        => _workflowService.EnqueueCaptureSymbolJobAsync(new CadenceEnqueueJobRequest(extractionId), cancellationToken);

    public Task<CadenceJobStatusResult> AllegroEnqueueFootprintJobAsync(
        long extractionId,
        CancellationToken cancellationToken = default)
        => _workflowService.EnqueueAllegroFootprintJobAsync(new CadenceEnqueueJobRequest(extractionId), cancellationToken);

    public Task<CadenceJobStatusResult> CadenceGetJobStatusAsync(
        long jobId,
        CancellationToken cancellationToken = default)
        => _workflowService.GetJobStatusAsync(jobId, cancellationToken);

    public Task<VerificationReportResult?> VerificationGetReportAsync(
        long? extractionId,
        long? jobId,
        CancellationToken cancellationToken = default)
        => _workflowService.GetVerificationReportAsync(new VerificationGetReportRequest(extractionId, jobId), cancellationToken);
}
