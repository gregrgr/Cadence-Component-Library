using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class ExternalImportSource : BaseEntity
{
    public string SourceName { get; set; } = null!;
    public ExternalImportSourceType SourceType { get; set; }
    public bool Enabled { get; set; }
    public string? Notes { get; set; }
}
