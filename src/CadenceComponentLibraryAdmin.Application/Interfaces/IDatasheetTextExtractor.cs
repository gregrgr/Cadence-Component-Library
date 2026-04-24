using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IDatasheetTextExtractor
{
    Task<DatasheetTextExtractionResult> ExtractTextAsync(
        DatasheetTextExtractionRequest request,
        CancellationToken cancellationToken = default);
}
