using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class LocalPdfTextExtractor : IDatasheetTextExtractor
{
    public Task<DatasheetTextExtractionResult> ExtractTextAsync(
        DatasheetTextExtractionRequest request,
        CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DatasheetAssetPath))
        {
            warnings.Add("No datasheet asset path is available for text extraction.");
            return Task.FromResult(new DatasheetTextExtractionResult(string.Empty, warnings));
        }

        // TODO: replace this placeholder with a safe PDF text extraction library that is
        // already approved for repository use. CI must remain independent of native PDF tools.
        warnings.Add("PDF text extraction is currently a placeholder implementation; no PDF parsing library is wired yet.");
        warnings.Add($"Datasheet path recorded for later extraction: {request.DatasheetAssetPath}");
        return Task.FromResult(new DatasheetTextExtractionResult(string.Empty, warnings));
    }
}
