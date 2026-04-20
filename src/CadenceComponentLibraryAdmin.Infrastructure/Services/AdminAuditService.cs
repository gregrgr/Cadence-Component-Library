using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Data;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class AdminAuditService : IAdminAuditService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminAuditService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(
        string action,
        string targetType,
        string targetId,
        string targetName,
        string? oldValue,
        string? newValue,
        string actor,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        _dbContext.AdminAuditLogs.Add(new AdminAuditLog
        {
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            TargetName = targetName,
            OldValue = oldValue,
            NewValue = newValue,
            Actor = actor,
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
