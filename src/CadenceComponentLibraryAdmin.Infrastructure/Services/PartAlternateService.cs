using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class PartAlternateService : IPartAlternateService
{
    private readonly ApplicationDbContext _dbContext;

    public PartAlternateService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RuleCheckResult> ValidateAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken = default)
    {
        var result = RuleCheckResult.Success();

        if (alternate.AltLevel != AlternateLevel.A)
        {
            return result;
        }

        var footprints = await _dbContext.CompanyParts
            .AsNoTracking()
            .Where(x => x.CompanyPN == alternate.SourceCompanyPN || x.CompanyPN == alternate.TargetCompanyPN)
            .Select(x => new { x.CompanyPN, x.DefaultFootprintName })
            .ToListAsync(cancellationToken);

        var source = footprints.FirstOrDefault(x => x.CompanyPN == alternate.SourceCompanyPN);
        var target = footprints.FirstOrDefault(x => x.CompanyPN == alternate.TargetCompanyPN);

        if (source is null || target is null)
        {
            result.AddError("Alternate validation failed because the source or target Company Part does not exist.");
            return result;
        }

        if (!string.Equals(source.DefaultFootprintName, target.DefaultFootprintName, StringComparison.OrdinalIgnoreCase))
        {
            result.AddError("AltLevel A requires the source and target Default Footprint to be identical.");
        }

        return result;
    }
}
