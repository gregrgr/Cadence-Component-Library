using System.Security.Cryptography;
using System.Text;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class ExternalImportTokenService : IExternalImportTokenService
{
    private readonly ApplicationDbContext _dbContext;

    public ExternalImportTokenService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExternalImportTokenCreationResult> CreateTokenAsync(
        ExternalImportTokenCreateRequest request,
        string createdByUserId,
        string createdByUserEmail,
        CancellationToken cancellationToken = default)
    {
        var rawToken = GenerateRawToken();
        var entity = new ExternalImportToken
        {
            TokenHash = HashToken(rawToken),
            DisplayName = request.DisplayName.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedByUserEmail = createdByUserEmail,
            SourceName = request.SourceName.Trim(),
            ExpiresAt = request.ExpiresAt,
            AllowedOrigins = Normalize(request.AllowedOrigins),
            Notes = Normalize(request.Notes),
            CreatedBy = createdByUserEmail
        };

        _dbContext.ExternalImportTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ExternalImportTokenCreationResult(entity, rawToken);
    }

    public async Task<ExternalImportTokenValidationResult> ValidateTokenAsync(
        string rawToken,
        string sourceName,
        string? origin,
        CancellationToken cancellationToken = default)
    {
        var normalizedToken = Normalize(rawToken);
        if (normalizedToken is null)
        {
            return new ExternalImportTokenValidationResult(false, "Import token is missing.", null, null);
        }

        var hash = HashToken(normalizedToken);
        var entity = await _dbContext.ExternalImportTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash && x.SourceName == sourceName, cancellationToken);

        if (entity is null)
        {
            return new ExternalImportTokenValidationResult(false, "Import token is invalid.", null, null);
        }

        if (entity.RevokedAt.HasValue)
        {
            return new ExternalImportTokenValidationResult(false, "Import token has been revoked.", entity, null);
        }

        if (entity.ExpiresAt <= DateTime.UtcNow)
        {
            return new ExternalImportTokenValidationResult(false, "Import token has expired.", entity, null);
        }

        if (!OriginAllowed(entity.AllowedOrigins, origin))
        {
            return new ExternalImportTokenValidationResult(false, "Import token origin is not allowed.", entity, null);
        }

        return new ExternalImportTokenValidationResult(true, null, entity, entity.CreatedByUserEmail);
    }

    public async Task RevokeTokenAsync(
        long tokenId,
        string revokedByUserId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ExternalImportTokens.FirstOrDefaultAsync(x => x.Id == tokenId, cancellationToken)
            ?? throw new InvalidOperationException($"Import token '{tokenId}' was not found.");

        entity.RevokedAt = DateTime.UtcNow;
        entity.RevokedByUserId = revokedByUserId;
        entity.UpdatedBy = actor;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkUsedAsync(long tokenId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ExternalImportTokens.FirstOrDefaultAsync(x => x.Id == tokenId, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.LastUsedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return $"eit_{token}";
    }

    private static bool OriginAllowed(string? allowedOrigins, string? origin)
    {
        var normalizedAllowedOrigins = Normalize(allowedOrigins);
        if (normalizedAllowedOrigins is null || string.IsNullOrWhiteSpace(origin))
        {
            return true;
        }

        var accepted = normalizedAllowedOrigins
            .Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return accepted.Contains(origin.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
