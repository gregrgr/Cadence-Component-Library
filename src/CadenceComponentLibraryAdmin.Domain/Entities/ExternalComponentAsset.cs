using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class ExternalComponentAsset : BaseEntity
{
    public string SourceName { get; set; } = null!;
    public long? ExternalComponentImportId { get; set; }
    public ExternalComponentImport? ExternalComponentImport { get; set; }
    public ExternalComponentAssetType AssetType { get; set; }
    public string? ExternalUuid { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public string? StoragePath { get; set; }
    public string? Url { get; set; }
    public string? Sha256 { get; set; }
    public long? SizeBytes { get; set; }
    public string? RawMetadataJson { get; set; }
}
