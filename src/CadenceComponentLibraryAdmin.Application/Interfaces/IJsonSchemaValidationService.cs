using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IJsonSchemaValidationService
{
    Task<JsonSchemaValidationResult> ValidateAsync(
        string schemaFileName,
        string json,
        CancellationToken cancellationToken = default);
}
