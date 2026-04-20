using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IPartAlternateService
{
    Task<RuleCheckResult> ValidateAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken = default);

    Task<RuleCheckResult> ValidateApprovalAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken = default);

    Task PrepareForSaveAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken = default);
}
