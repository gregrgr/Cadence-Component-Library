using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IPartAlternateService
{
    Task<RuleCheckResult> ValidateAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken = default);
}
