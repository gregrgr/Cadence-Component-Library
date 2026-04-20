using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class AlternatesIndexViewModel
{
    public required PagedResult<AlternateListItemViewModel> Results { get; init; }
    public string? SourceCompanyPN { get; init; }
    public string? TargetCompanyPN { get; init; }
    public AlternateLevel? AltLevel { get; init; }
}
