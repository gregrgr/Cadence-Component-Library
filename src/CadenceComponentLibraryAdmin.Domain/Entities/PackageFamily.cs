using CadenceComponentLibraryAdmin.Domain.Common;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class PackageFamily : BaseEntity
{
    public string PackageFamilyCode { get; set; } = null!;
    public string MountType { get; set; } = null!;
    public int LeadCount { get; set; }
    public decimal? BodyLmm { get; set; }
    public decimal? BodyWmm { get; set; }
    public decimal? PitchMm { get; set; }
    public decimal? EPLmm { get; set; }
    public decimal? EPWmm { get; set; }
    public string? DensityLevel { get; set; }
    public string? PackageStd { get; set; }
    public string? Notes { get; set; }
    public string PackageSignature { get; set; } = null!;

    public ICollection<FootprintVariant> FootprintVariants { get; set; } = new List<FootprintVariant>();
}
