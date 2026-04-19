using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IChangeLogService
{
    Task WriteAsync(
        string companyPn,
        ChangeType changeType,
        string? oldValue,
        string? newValue,
        string? reason,
        string changedBy,
        string? releaseName = null,
        CancellationToken cancellationToken = default);

    Task<List<PartChangeLog>> QueryAsync(
        ChangeLogQuery query,
        CancellationToken cancellationToken = default);
}
