using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class ChangeLogService : IChangeLogService
{
    private readonly ApplicationDbContext _dbContext;

    public ChangeLogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task WriteAsync(
        string companyPn,
        ChangeType changeType,
        string? oldValue,
        string? newValue,
        string? reason,
        string changedBy,
        string? releaseName = null,
        CancellationToken cancellationToken = default)
    {
        var item = new PartChangeLog
        {
            CompanyPN = companyPn,
            ChangeType = changeType,
            OldValue = oldValue,
            NewValue = newValue,
            Reason = reason,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            ReleaseName = releaseName,
            CreatedBy = changedBy
        };

        _dbContext.PartChangeLogs.Add(item);
        return Task.CompletedTask;
    }

    public async Task<List<PartChangeLog>> QueryAsync(
        ChangeLogQuery query,
        CancellationToken cancellationToken = default)
    {
        var logs = _dbContext.PartChangeLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.CompanyPN))
        {
            logs = logs.Where(x => x.CompanyPN.Contains(query.CompanyPN));
        }

        if (query.ChangeType.HasValue)
        {
            logs = logs.Where(x => x.ChangeType == query.ChangeType.Value);
        }

        if (query.ChangedFrom.HasValue)
        {
            logs = logs.Where(x => x.ChangedAt >= query.ChangedFrom.Value);
        }

        if (query.ChangedTo.HasValue)
        {
            logs = logs.Where(x => x.ChangedAt <= query.ChangedTo.Value);
        }

        return await logs
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
