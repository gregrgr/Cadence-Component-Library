using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Librarian")]
[Route("ExternalImportTokens")]
public sealed class ExternalImportTokensController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IExternalImportTokenService _externalImportTokenService;
    private readonly IAdminAuditService _adminAuditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExternalImportTokensController(
        ApplicationDbContext dbContext,
        IExternalImportTokenService externalImportTokenService,
        IAdminAuditService adminAuditService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _externalImportTokenService = externalImportTokenService;
        _adminAuditService = adminAuditService;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var utcNow = DateTime.UtcNow;
        var items = await _dbContext.ExternalImportTokens
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ExternalImportTokenListItemViewModel
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                SourceName = x.SourceName,
                CreatedByUserEmail = x.CreatedByUserEmail,
                ExpiresAt = x.ExpiresAt,
                LastUsedAt = x.LastUsedAt,
                RevokedAt = x.RevokedAt,
                AllowedOrigins = x.AllowedOrigins,
                Notes = x.Notes,
                IsExpired = x.ExpiresAt <= utcNow,
                IsRevoked = x.RevokedAt.HasValue
            })
            .ToListAsync();

        return View(new ExternalImportTokensIndexViewModel
        {
            Tokens = items
        });
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new ExternalImportTokenCreateViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExternalImportTokenCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.ExpiresAt <= DateTime.UtcNow)
        {
            ModelState.AddModelError(nameof(model.ExpiresAt), "Expiration must be in the future.");
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var result = await _externalImportTokenService.CreateTokenAsync(
            new ExternalImportTokenCreateRequest(
                model.DisplayName,
                model.SourceName,
                DateTime.SpecifyKind(model.ExpiresAt, DateTimeKind.Utc),
                model.AllowedOrigins,
                model.Notes),
            user.Id,
            user.Email ?? user.UserName ?? user.Id);

        await _adminAuditService.WriteAsync(
            "ExternalImportTokenCreated",
            "ExternalImportToken",
            result.Token.Id.ToString(),
            result.Token.DisplayName,
            null,
            $"Source={result.Token.SourceName}; ExpiresAt={result.Token.ExpiresAt:O}",
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["CreatedRawToken"] = result.RawToken;
        TempData["CreatedDisplayName"] = result.Token.DisplayName;
        TempData["CreatedSourceName"] = result.Token.SourceName;
        TempData["CreatedExpiresAt"] = result.Token.ExpiresAt.ToString("O");
        TempData["CreatedAllowedOrigins"] = result.Token.AllowedOrigins;
        return RedirectToAction(nameof(CreatedToken));
    }

    [HttpGet("Created")]
    public IActionResult CreatedToken()
    {
        if (TempData["CreatedRawToken"] is not string rawToken ||
            TempData["CreatedDisplayName"] is not string displayName ||
            TempData["CreatedSourceName"] is not string sourceName ||
            TempData["CreatedExpiresAt"] is not string expiresAtRaw ||
            !DateTime.TryParse(expiresAtRaw, out var expiresAt))
        {
            TempData["ErrorMessage"] = "The raw import token can only be shown immediately after creation.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ExternalImportTokenCreatedViewModel
        {
            RawToken = rawToken,
            DisplayName = displayName,
            SourceName = sourceName,
            ExpiresAt = expiresAt,
            AllowedOrigins = TempData["CreatedAllowedOrigins"] as string
        });
    }

    [HttpPost("{id:long}/Revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var token = await _dbContext.ExternalImportTokens.FirstOrDefaultAsync(x => x.Id == id);
        if (token is null)
        {
            return NotFound();
        }

        await _externalImportTokenService.RevokeTokenAsync(id, user.Id, GetActor());
        await _adminAuditService.WriteAsync(
            "ExternalImportTokenRevoked",
            "ExternalImportToken",
            token.Id.ToString(),
            token.DisplayName,
            token.RevokedAt?.ToString("O"),
            DateTime.UtcNow.ToString("O"),
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "Import token revoked.";
        return RedirectToAction(nameof(Index));
    }

    private string GetActor() => User.Identity?.Name ?? "system";

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetUserAgent() => Request.Headers.UserAgent.ToString();
}
