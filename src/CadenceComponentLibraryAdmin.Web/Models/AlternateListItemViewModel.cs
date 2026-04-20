using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class AlternateListItemViewModel
{
    public required PartAlternate Alternate { get; init; }
    public string? SourceDescription { get; init; }
    public string? TargetDescription { get; init; }
}
