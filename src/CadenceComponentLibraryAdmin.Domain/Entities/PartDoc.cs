using CadenceComponentLibraryAdmin.Domain.Common;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class PartDoc : BaseEntity
{
    public string CompanyPN { get; set; } = null!;
    public CompanyPart CompanyPart { get; set; } = null!;
    public string DocType { get; set; } = null!;
    public string? DocUrl { get; set; }
    public string? LocalPath { get; set; }
    public string? VersionTag { get; set; }
    public string? SourceProvider { get; set; }
}
