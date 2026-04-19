using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class PackageFamilyService : IPackageFamilyService
{
    private readonly ApplicationDbContext _dbContext;

    public PackageFamilyService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string BuildPackageSignature(PackageFamily packageFamily)
    {
        return string.Join("|",
            packageFamily.MountType?.Trim() ?? "NA",
            packageFamily.LeadCount.ToString(),
            Format(packageFamily.BodyLmm),
            Format(packageFamily.BodyWmm),
            Format(packageFamily.PitchMm, "NA"),
            Format(packageFamily.EPLmm, "0.00"),
            Format(packageFamily.EPWmm, "0.00"));
    }

    public async Task<RuleCheckResult> PrepareForSaveAsync(
        PackageFamily packageFamily,
        long? existingId = null,
        CancellationToken cancellationToken = default)
    {
        packageFamily.PackageSignature = BuildPackageSignature(packageFamily);

        var result = RuleCheckResult.Success();

        var duplicate = await _dbContext.PackageFamilies
            .AsNoTracking()
            .Where(x => x.PackageSignature == packageFamily.PackageSignature)
            .Where(x => !existingId.HasValue || x.Id != existingId.Value)
            .Select(x => x.PackageFamilyCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(duplicate))
        {
            result.AddError($"Package Signature already exists. Reuse existing Package Family: {duplicate}");
        }

        return result;
    }

    private static string Format(decimal? value, string nullValue = "NA")
    {
        return value.HasValue ? value.Value.ToString("0.00") : nullValue;
    }
}
