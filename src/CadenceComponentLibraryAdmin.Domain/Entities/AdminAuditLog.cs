namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class AdminAuditLog
{
    public long Id { get; set; }
    public string Action { get; set; } = null!;
    public string TargetType { get; set; } = null!;
    public string TargetId { get; set; } = null!;
    public string TargetName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Actor { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
