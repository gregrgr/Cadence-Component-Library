using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class ExternalImportsIndexViewModel
{
    public PagedResult<ExternalImportListItemViewModel> Result { get; init; } = new();
    public string? Search { get; init; }
    public string? LcscId { get; init; }
    public string? Manufacturer { get; init; }
    public string? ManufacturerPn { get; init; }
    public string? Symbol { get; init; }
    public string? Footprint { get; init; }
    public string? Model3D { get; init; }
    public ExternalImportStatus? ImportStatus { get; init; }
    public bool HasDatasheet { get; init; }
    public bool HasManual { get; init; }
    public bool HasStep { get; init; }
    public bool Has3D { get; init; }
    public bool HasThumbnail { get; init; }
    public bool DuplicateWarning { get; init; }
}

public sealed class ExternalImportListItemViewModel
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? Manufacturer { get; init; }
    public string? ManufacturerPN { get; init; }
    public string? LcscId { get; init; }
    public string? Supplier { get; init; }
    public string? SupplierId { get; init; }
    public string? SymbolName { get; init; }
    public string? FootprintName { get; init; }
    public string? Model3DName { get; init; }
    public bool HasDatasheet { get; init; }
    public bool HasManual { get; init; }
    public bool HasStep { get; init; }
    public bool HasRawJson { get; init; }
    public bool HasThumbnail { get; init; }
    public string? DuplicateWarning { get; init; }
    public ExternalImportStatus ImportStatus { get; init; }
    public long? ThumbnailAssetId { get; init; }
}

public sealed class ExternalImportDetailsViewModel
{
    public ExternalComponentImport Import { get; init; } = null!;
    public IReadOnlyList<ExternalComponentAsset> Assets { get; init; } = [];
}
