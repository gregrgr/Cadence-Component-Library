using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class NoOpChangeLogService : IChangeLogService
{
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
        return Task.CompletedTask;
    }

    public Task<List<PartChangeLog>> QueryAsync(ChangeLogQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<PartChangeLog>());
    }
}
