using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class CompanyPartService : ICompanyPartService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IChangeLogService _changeLogService;

    public CompanyPartService(ApplicationDbContext dbContext, IChangeLogService changeLogService)
    {
        _dbContext = dbContext;
        _changeLogService = changeLogService;
    }

    public async Task<RuleCheckResult> ValidateApprovalAsync(
        CompanyPart companyPart,
        CancellationToken cancellationToken = default)
    {
        var result = RuleCheckResult.Success();

        if (companyPart.ApprovalStatus != ApprovalStatus.Approved)
        {
            return result;
        }

        if (string.IsNullOrWhiteSpace(companyPart.DatasheetUrl))
        {
            result.AddError("Approved parts must have a Datasheet URL.");
        }

        var hasApprovedMpn = await _dbContext.ManufacturerParts
            .AsNoTracking()
            .AnyAsync(x => x.CompanyPN == companyPart.CompanyPN && x.IsApproved, cancellationToken);

        if (!hasApprovedMpn)
        {
            result.AddError("Approved parts must have at least one approved Manufacturer Part.");
        }

        var symbolFamilyExists = await _dbContext.SymbolFamilies
            .AsNoTracking()
            .AnyAsync(x => x.SymbolFamilyCode == companyPart.SymbolFamilyCode, cancellationToken);

        if (!symbolFamilyExists)
        {
            result.AddError("Approved parts must reference a valid Symbol Family.");
        }

        var packageFamilyExists = await _dbContext.PackageFamilies
            .AsNoTracking()
            .AnyAsync(x => x.PackageFamilyCode == companyPart.PackageFamilyCode, cancellationToken);

        if (!packageFamilyExists)
        {
            result.AddError("Approved parts must reference a valid Package Family.");
        }

        var footprint = await _dbContext.FootprintVariants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FootprintName == companyPart.DefaultFootprintName, cancellationToken);

        if (footprint is null)
        {
            result.AddError("Approved parts must reference a valid Default Footprint.");
        }
        else if (footprint.Status != FootprintStatus.Released)
        {
            result.AddError("Approved parts can only use Released footprint variants.");
        }

        return result;
    }

    public async Task<RuleCheckResult> ApplyEditRulesAsync(
        CompanyPart existing,
        CompanyPart incoming,
        string changedBy,
        CancellationToken cancellationToken = default)
    {
        var result = await ValidateApprovalAsync(incoming, cancellationToken);

        if (!result.Succeeded)
        {
            return result;
        }

        if (existing.ApprovalStatus == ApprovalStatus.Approved &&
            !string.Equals(existing.DefaultFootprintName, incoming.DefaultFootprintName, StringComparison.OrdinalIgnoreCase))
        {
            await _changeLogService.WriteAsync(
                existing.CompanyPN,
                ChangeType.FootprintChanged,
                existing.DefaultFootprintName,
                incoming.DefaultFootprintName,
                "Approved part footprint updated.",
                changedBy,
                cancellationToken: cancellationToken);
        }

        if (existing.ApprovalStatus == ApprovalStatus.Approved &&
            !string.Equals(existing.SymbolFamilyCode, incoming.SymbolFamilyCode, StringComparison.OrdinalIgnoreCase))
        {
            await _changeLogService.WriteAsync(
                existing.CompanyPN,
                ChangeType.SymbolChanged,
                existing.SymbolFamilyCode,
                incoming.SymbolFamilyCode,
                "Approved part symbol family updated.",
                changedBy,
                cancellationToken: cancellationToken);
        }

        return result;
    }
}
