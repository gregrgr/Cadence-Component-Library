namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IAdminAuditService
{
    Task WriteAsync(
        string action,
        string targetType,
        string targetId,
        string targetName,
        string? oldValue,
        string? newValue,
        string actor,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}
