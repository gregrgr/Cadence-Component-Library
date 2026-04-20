using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class CompanyPartDetailsViewModel
{
    public required CompanyPart CompanyPart { get; init; }
    public IReadOnlyList<ManufacturerPart> ApprovedManufacturerParts { get; init; } = [];
    public IReadOnlyList<AlternateListItemViewModel> SourceAlternates { get; init; } = [];
    public IReadOnlyList<AlternateListItemViewModel> TargetAlternates { get; init; } = [];
}
