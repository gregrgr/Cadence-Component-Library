using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Web.Controllers.Api;

[ApiController]
[Route("api/import/easyeda")]
public sealed class EasyEdaImportController : ControllerBase
{
    private readonly IExternalImportService _externalImportService;
    private readonly IExternalImportTokenService _externalImportTokenService;
    private readonly ExternalImportOptions _options;
    private readonly IHostEnvironment _environment;

    public EasyEdaImportController(
        IExternalImportService externalImportService,
        IExternalImportTokenService externalImportTokenService,
        IOptions<ExternalImportOptions> options,
        IHostEnvironment environment)
    {
        _externalImportService = externalImportService;
        _externalImportTokenService = externalImportTokenService;
        _options = options.Value;
        _environment = environment;
    }

    [HttpPost("component")]
    public async Task<IActionResult> ImportComponent(
        [FromBody] EasyEdaComponentImportRequest request,
        CancellationToken cancellationToken)
    {
        var authorization = await ValidateImportAuthorizationAsync(request.SourceName, cancellationToken);
        if (!authorization.IsAuthorized)
        {
            return Unauthorized();
        }

        var result = await _externalImportService.UpsertEasyEdaComponentAsync(
            request,
            actor: authorization.ActorEmail ?? "easyeda-import-token",
            cancellationToken);

        if (authorization.TokenId.HasValue)
        {
            await _externalImportTokenService.MarkUsedAsync(authorization.TokenId.Value, cancellationToken);
        }

        return Ok(new
        {
            importId = result.ImportId,
            duplicateWarnings = result.DuplicateWarnings,
            missingCriticalFields = result.MissingCriticalFields,
            normalizedFieldSummary = result.Summary
        });
    }

    [HttpPost("component/{id:long}/asset")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadAsset(
        long id,
        [FromForm] string assetType,
        [FromForm] IFormFile? file,
        [FromForm] string? externalUuid,
        [FromForm] string? url,
        [FromForm] string? rawMetadataJson,
        CancellationToken cancellationToken)
    {
        var authorization = await ValidateImportAuthorizationAsync("EasyEDA Pro", cancellationToken);
        if (!authorization.IsAuthorized)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<ExternalComponentAssetType>(assetType, true, out var parsedAssetType))
        {
            return BadRequest(new { error = "Unsupported assetType." });
        }

        var asset = await _externalImportService.SaveAssetAsync(
            id,
            new ExternalImportAssetUpload(
                parsedAssetType,
                file?.OpenReadStream(),
                file?.FileName,
                file?.FileName,
                file?.ContentType,
                file?.Length,
                externalUuid,
                url,
                rawMetadataJson),
            actor: authorization.ActorEmail ?? "easyeda-import-token",
            cancellationToken);

        if (authorization.TokenId.HasValue)
        {
            await _externalImportTokenService.MarkUsedAsync(authorization.TokenId.Value, cancellationToken);
        }

        return Ok(new
        {
            assetId = asset.Id,
            assetType = asset.AssetType,
            sha256 = asset.Sha256,
            sizeBytes = asset.SizeBytes,
            storagePath = asset.StoragePath,
            url = asset.Url
        });
    }

    [HttpPost("component/{id:long}/create-candidate")]
    [Authorize(Roles = "Admin,Librarian,EEReviewer")]
    public async Task<IActionResult> CreateCandidate(long id, CancellationToken cancellationToken)
    {
        var candidate = await _externalImportService.CreateCandidateAsync(id, User.Identity?.Name ?? "system", cancellationToken);
        return Ok(new
        {
            candidateId = candidate.Id,
            status = candidate.CandidateStatus.ToString()
        });
    }

    private async Task<ImportAuthorizationResult> ValidateImportAuthorizationAsync(string? sourceName, CancellationToken cancellationToken)
    {
        if (Request.Headers.TryGetValue("X-Import-Token", out var providedToken))
        {
            var validation = await _externalImportTokenService.ValidateTokenAsync(
                providedToken.ToString(),
                string.IsNullOrWhiteSpace(sourceName) ? "EasyEDA Pro" : sourceName.Trim(),
                Request.Headers.Origin.ToString(),
                cancellationToken);

            return new ImportAuthorizationResult(validation.IsValid, validation.Token?.Id, validation.ActorEmail);
        }

        var expectedApiKey = _options.EasyEdaApiKey;
        if (_environment.IsDevelopment() &&
            _options.AllowLegacyApiKeyInDevelopment &&
            !string.IsNullOrWhiteSpace(expectedApiKey) &&
            Request.Headers.TryGetValue("X-Import-Api-Key", out var providedApiKey) &&
            string.Equals(expectedApiKey, providedApiKey.ToString(), StringComparison.Ordinal))
        {
            return new ImportAuthorizationResult(true, null, "legacy-api-key");
        }

        return new ImportAuthorizationResult(false, null, null);
    }

    private sealed record ImportAuthorizationResult(bool IsAuthorized, long? TokenId, string? ActorEmail);
}
