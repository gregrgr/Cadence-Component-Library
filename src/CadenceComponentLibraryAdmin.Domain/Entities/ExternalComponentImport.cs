using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class ExternalComponentImport : BaseEntity
{
    public string SourceName { get; set; } = null!;

    public string? ExternalDeviceUuid { get; set; }
    public string? ExternalLibraryUuid { get; set; }
    public string? SearchKeyword { get; set; }
    public string? LcscId { get; set; }
    public string ImportKey { get; set; } = null!;

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ClassificationJson { get; set; }
    public string? Manufacturer { get; set; }
    public string? ManufacturerPN { get; set; }
    public string? Supplier { get; set; }
    public string? SupplierId { get; set; }

    public string? SymbolName { get; set; }
    public string? SymbolUuid { get; set; }
    public string? SymbolLibraryUuid { get; set; }
    public string? SymbolType { get; set; }
    public string? SymbolRawJson { get; set; }

    public string? FootprintName { get; set; }
    public string? FootprintUuid { get; set; }
    public string? FootprintLibraryUuid { get; set; }
    public string? FootprintRawJson { get; set; }
    public long? FootprintRenderAssetId { get; set; }
    public ExternalComponentAsset? FootprintRenderAsset { get; set; }

    public string? ImageUuidsJson { get; set; }

    public string? Model3DName { get; set; }
    public string? Model3DUuid { get; set; }
    public string? Model3DLibraryUuid { get; set; }
    public string? Model3DRawJson { get; set; }

    public string? DatasheetUrl { get; set; }
    public string? ManualUrl { get; set; }
    public string? StepUrl { get; set; }
    public long? StepAssetId { get; set; }
    public ExternalComponentAsset? StepAsset { get; set; }
    public long? DatasheetAssetId { get; set; }
    public ExternalComponentAsset? DatasheetAsset { get; set; }
    public long? ManualAssetId { get; set; }
    public ExternalComponentAsset? ManualAsset { get; set; }

    public long? JlcInventory { get; set; }
    public decimal? JlcPrice { get; set; }
    public long? LcscInventory { get; set; }
    public decimal? LcscPrice { get; set; }

    public string? SearchItemRawJson { get; set; }
    public string? DeviceItemRawJson { get; set; }
    public string? DeviceAssociationRawJson { get; set; }
    public string? DevicePropertyRawJson { get; set; }
    public string? OtherPropertyRawJson { get; set; }
    public string? FullRawJson { get; set; }
    public string? LcscRawJson { get; set; }
    public DateTime? LcscEnrichedAt { get; set; }
    public LcscEnrichmentStatus LcscEnrichmentStatus { get; set; }
    public string? LcscEnrichmentMessage { get; set; }

    public ExternalImportStatus ImportStatus { get; set; }
    public string? DuplicateWarning { get; set; }
    public long? CandidateId { get; set; }
    public OnlineCandidate? Candidate { get; set; }
    public DateTime LastImportedAt { get; set; }

    public ICollection<ExternalComponentAsset> Assets { get; set; } = new List<ExternalComponentAsset>();
}
