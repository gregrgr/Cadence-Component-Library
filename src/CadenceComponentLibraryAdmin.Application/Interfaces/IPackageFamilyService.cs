using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IPackageFamilyService
{
    string BuildPackageSignature(PackageFamily packageFamily);

    Task<RuleCheckResult> PrepareForSaveAsync(
        PackageFamily packageFamily,
        long? existingId = null,
        CancellationToken cancellationToken = default);
}
