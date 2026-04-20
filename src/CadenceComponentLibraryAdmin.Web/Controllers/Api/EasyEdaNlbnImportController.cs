using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Web.Controllers.Api;

[ApiController]
[Route("api/import/easyeda-nlbn")]
public sealed class EasyEdaNlbnImportController : ControllerBase
{
    private readonly INlbnEasyEdaClient _nlbnEasyEdaClient;
    private readonly ExternalImportOptions _options;

    public EasyEdaNlbnImportController(
        INlbnEasyEdaClient nlbnEasyEdaClient,
        IOptions<ExternalImportOptions> options)
    {
        _nlbnEasyEdaClient = nlbnEasyEdaClient;
        _options = options.Value;
    }

    [HttpPost("lcsc/{lcscId}")]
    public async Task<IActionResult> ImportByLcscId(
        string lcscId,
        [FromBody] EasyEdaNlbnImportRequest? request,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var isAllowedRole =
                User.IsInRole("Admin") ||
                User.IsInRole("Librarian") ||
                User.IsInRole("EEReviewer");
            if (!isAllowedRole)
            {
                return Forbid();
            }
        }
        else
        {
            if (!HasValidImportToken())
            {
                return Unauthorized();
            }
        }

        var import = await _nlbnEasyEdaClient.ImportByLcscIdAsync(
            lcscId,
            new NlbnImportOptions(
                request?.DownloadStep,
                request?.DownloadObj,
                request?.GeneratePreview),
            User.Identity?.Name ?? "easyeda-nlbn-api",
            cancellationToken);

        return Ok(new
        {
            importId = import.Id,
            import.LcscId,
            import.Name,
            import.Manufacturer,
            import.ManufacturerPN,
            import.PackageName,
            import.ImportStatus
        });
    }

    private bool HasValidImportToken()
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

public sealed class EasyEdaNlbnImportRequest
{
    public bool? DownloadStep { get; init; }
    public bool? DownloadObj { get; init; }
    public bool? GeneratePreview { get; init; }
}
