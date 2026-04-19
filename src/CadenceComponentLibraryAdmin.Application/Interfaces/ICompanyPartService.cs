using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface ICompanyPartService
{
    Task<RuleCheckResult> ValidateApprovalAsync(
        CompanyPart companyPart,
        CancellationToken cancellationToken = default);

    Task<RuleCheckResult> ApplyEditRulesAsync(
        CompanyPart existing,
        CompanyPart incoming,
        string changedBy,
        CancellationToken cancellationToken = default);
}
