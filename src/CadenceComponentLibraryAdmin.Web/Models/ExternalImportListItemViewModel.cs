using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class ExternalImportsIndexViewModel
{
    public PagedResult<ExternalImportListItemViewModel> Result { get; init; } = new();
    public ExternalImportFromLcscInputModel ImportForm { get; init; } = new();
    public ExternalImportBatchInputModel BatchImportForm { get; init; } = new();
    public string? Search { get; init; }
    public string? LcscId { get; init; }
    public string? Manufacturer { get; init; }
    public string? ManufacturerPn { get; init; }
    public string? Symbol { get; init; }
    public string? Footprint { get; init; }
    public string? Model3D { get; init; }
    public ExternalImportStatus? ImportStatus { get; init; }
    public bool HasDatasheet { get; init; }
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
    public string? PackageName { get; init; }
    public string? Model3DName { get; init; }
    public bool HasDatasheet { get; init; }
    public bool HasStep { get; init; }
    public bool HasObj { get; init; }
    public bool HasSymbolRaw { get; init; }
    public bool HasFootprintRaw { get; init; }
    public bool HasRawJson { get; init; }
    public bool HasPreview { get; init; }
    public bool Has3DModel { get; init; }
    public string? DuplicateWarning { get; init; }
    public ExternalImportStatus ImportStatus { get; init; }
    public long? PreviewAssetId { get; init; }
}

public sealed class ExternalImportDetailsViewModel
{
    public ExternalComponentImport Import { get; init; } = null!;
    public IReadOnlyList<ExternalComponentAsset> Assets { get; init; } = [];
}

public sealed class ExternalImportFromLcscInputModel
{
    public string? LcscId { get; set; }
    public bool DownloadStep { get; set; }
    public bool DownloadObj { get; set; }
    public bool GeneratePreview { get; set; } = true;
}

public sealed class ExternalImportBatchInputModel
{
    public string? LcscIds { get; set; }
    public bool ContinueOnError { get; set; }
    public int? MaxParallelImports { get; set; }
    public bool DownloadStep { get; set; }
    public bool GeneratePreview { get; set; } = true;
}
