using CadenceComponentLibraryAdmin.Domain.Common;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class ExternalImportToken : BaseEntity
{
    public string TokenHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public string CreatedByUserEmail { get; set; } = null!;
    public string SourceName { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByUserId { get; set; }
    public string? AllowedOrigins { get; set; }
    public string? Notes { get; set; }
}
