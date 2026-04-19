using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class FootprintVariant : BaseEntity
{
    public string FootprintName { get; set; } = null!;
    public string PackageFamilyCode { get; set; } = null!;
    public PackageFamily PackageFamily { get; set; } = null!;
    public string PsmPath { get; set; } = null!;
    public string? DraPath { get; set; }
    public string? PadstackSet { get; set; }
    public string? StepPath { get; set; }
    public string VariantType { get; set; } = null!;
    public FootprintStatus Status { get; set; }
}
