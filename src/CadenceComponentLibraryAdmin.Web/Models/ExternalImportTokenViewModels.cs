using System.ComponentModel.DataAnnotations;
using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class ExternalImportTokenListItemViewModel
{
    public long Id { get; init; }
    public required string DisplayName { get; init; }
    public required string SourceName { get; init; }
    public required string CreatedByUserEmail { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? LastUsedAt { get; init; }
    public DateTime? RevokedAt { get; init; }
    public string? AllowedOrigins { get; init; }
    public string? Notes { get; init; }
    public bool IsExpired { get; init; }
    public bool IsRevoked { get; init; }
}

public sealed class ExternalImportTokensIndexViewModel
{
    public IReadOnlyList<ExternalImportTokenListItemViewModel> Tokens { get; init; } = [];
}

public sealed class ExternalImportTokenCreateViewModel
{
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = "EasyEDA Pro Connector";

    [Required]
    [StringLength(120)]
    public string SourceName { get; set; } = "EasyEDA Pro";

    [Display(Name = "Expires At (UTC)")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);

    [StringLength(2000)]
    [Display(Name = "Allowed Origins")]
    public string? AllowedOrigins { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public sealed class ExternalImportTokenCreatedViewModel
{
    public required string RawToken { get; init; }
    public required string DisplayName { get; init; }
    public required string SourceName { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string? AllowedOrigins { get; init; }
}
