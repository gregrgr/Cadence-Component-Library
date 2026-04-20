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

        if (string.IsNullOrWhiteSpace(alternate.SourceCompanyPN))
        {
            result.AddError("Source Company Part is required.");
        }

        if (string.IsNullOrWhiteSpace(alternate.TargetCompanyPN))
        {
            result.AddError("Target Company Part is required.");
        }

        if (!result.Succeeded)
        {
            return result;
        }

        if (string.Equals(alternate.SourceCompanyPN, alternate.TargetCompanyPN, StringComparison.OrdinalIgnoreCase))
        {
            result.AddError("Source and target Company Parts cannot be the same.");
        }

        var parts = await LoadPartPairAsync(alternate, cancellationToken);
        var source = parts.Source;
        var target = parts.Target;

        if (source is null)
        {
            result.AddError("Source Company Part does not exist.");
        }

        if (target is null)
        {
            result.AddError("Target Company Part does not exist.");
        }

        if (source is null || target is null)
        {
            return result;
        }

        if (alternate.AltLevel != AlternateLevel.A)
        {
            return result;
        }

        if (!string.Equals(source.DefaultFootprintName, target.DefaultFootprintName, StringComparison.OrdinalIgnoreCase))
        {
            result.AddError("AltLevel A requires the source and target Default Footprint to be identical.");
        }

        return result;
    }

    public async Task<RuleCheckResult> ValidateApprovalAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken = default)
    {
        var result = await ValidateAsync(alternate, cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        var parts = await LoadPartPairAsync(alternate, cancellationToken);
        if (parts.Source is null || parts.Target is null)
        {
            return result;
        }

        if (parts.Source.ApprovalStatus != ApprovalStatus.Approved)
        {
            result.AddError("Source Company Part must be approved before the alternate relation can be approved.");
        }

        if (parts.Target.ApprovalStatus != ApprovalStatus.Approved)
        {
            result.AddError("Target Company Part must be approved before the alternate relation can be approved.");
        }

        return result;
    }

    public async Task PrepareForSaveAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken = default)
    {
        var parts = await LoadPartPairAsync(alternate, cancellationToken);
        if (parts.Source is null || parts.Target is null)
        {
            return;
        }

        alternate.SameFootprintYN = string.Equals(
            parts.Source.DefaultFootprintName,
            parts.Target.DefaultFootprintName,
            StringComparison.OrdinalIgnoreCase);

        alternate.SameSymbolYN = string.Equals(
            parts.Source.SymbolFamilyCode,
            parts.Target.SymbolFamilyCode,
            StringComparison.OrdinalIgnoreCase);

        alternate.NeedLayoutReviewYN = !alternate.SameFootprintYN;
        alternate.NeedEEReviewYN = !alternate.SameSymbolYN;
    }

    private async Task<(CompanyPart? Source, CompanyPart? Target)> LoadPartPairAsync(
        PartAlternate alternate,
        CancellationToken cancellationToken)
    {
        var parts = await _dbContext.CompanyParts
            .AsNoTracking()
            .Where(x => x.CompanyPN == alternate.SourceCompanyPN || x.CompanyPN == alternate.TargetCompanyPN)
            .ToListAsync(cancellationToken);

        var source = parts.FirstOrDefault(x => x.CompanyPN == alternate.SourceCompanyPN);
        var target = parts.FirstOrDefault(x => x.CompanyPN == alternate.TargetCompanyPN);
        return (source, target);
    }
}
