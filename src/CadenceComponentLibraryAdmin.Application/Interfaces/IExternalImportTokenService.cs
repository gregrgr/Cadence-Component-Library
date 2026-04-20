using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IExternalImportTokenService
{
    Task<ExternalImportTokenCreationResult> CreateTokenAsync(
        ExternalImportTokenCreateRequest request,
        string createdByUserId,
        string createdByUserEmail,
        CancellationToken cancellationToken = default);

    Task<ExternalImportTokenValidationResult> ValidateTokenAsync(
        string rawToken,
        string sourceName,
        string? origin,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(
        long tokenId,
        string revokedByUserId,
        string actor,
        CancellationToken cancellationToken = default);

    Task MarkUsedAsync(long tokenId, CancellationToken cancellationToken = default);
}

public sealed record ExternalImportTokenCreateRequest(
    string DisplayName,
    string SourceName,
    DateTime ExpiresAt,
    string? AllowedOrigins,
    string? Notes);

public sealed record ExternalImportTokenCreationResult(
    ExternalImportToken Token,
    string RawToken);

public sealed record ExternalImportTokenValidationResult(
    bool IsValid,
    string? FailureReason,
    ExternalImportToken? Token,
    string? ActorEmail);
