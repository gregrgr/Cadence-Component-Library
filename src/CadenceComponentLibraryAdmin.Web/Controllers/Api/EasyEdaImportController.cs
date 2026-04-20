using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Web.Controllers.Api;

[ApiController]
[Route("api/import/easyeda")]
[Obsolete("Deprecated in B4. Use /api/import/easyeda-nlbn/lcsc/{lcscId} or /ExternalImports/ImportFromLcsc.")]
public sealed class EasyEdaImportController : ControllerBase
{
    private readonly IExternalImportService _externalImportService;
    private readonly ExternalImportOptions _options;

    public EasyEdaImportController(
        IExternalImportService externalImportService,
        IOptions<ExternalImportOptions> options)
    {
        _externalImportService = externalImportService;
        _options = options.Value;
    }

    [HttpPost("component")]
    public async Task<IActionResult> ImportComponent(
        [FromBody] EasyEdaComponentImportRequest request,
        CancellationToken cancellationToken)
    {
        if (!HasValidApiKey())
        {
            return Unauthorized();
        }

        var result = await _externalImportService.UpsertEasyEdaComponentAsync(
            request,
            actor: "easyeda-import-api",
            cancellationToken);

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
        if (!HasValidApiKey())
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
            actor: "easyeda-import-api",
            cancellationToken);

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

    private bool HasValidApiKey()
    {
        var expectedApiKey = _options.EasyEdaApiKey;
        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue("X-Import-Api-Key", out var providedApiKey))
        {
            return false;
        }

        return string.Equals(expectedApiKey, providedApiKey.ToString(), StringComparison.Ordinal);
    }
}
