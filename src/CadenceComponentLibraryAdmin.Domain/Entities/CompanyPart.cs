using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class CompanyPart : BaseEntity
{
    public string CompanyPN { get; set; } = null!;
    public string PartClass { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? ValueNorm { get; set; }

    public string SymbolFamilyCode { get; set; } = null!;
    public SymbolFamily SymbolFamily { get; set; } = null!;

    public string PackageFamilyCode { get; set; } = null!;
    public PackageFamily PackageFamily { get; set; } = null!;

    public string DefaultFootprintName { get; set; } = null!;
    public FootprintVariant DefaultFootprint { get; set; } = null!;

    public ApprovalStatus ApprovalStatus { get; set; }
    public LifecycleStatus LifecycleStatus { get; set; }

    public string? AltGroup { get; set; }
    public bool PreferredYN { get; set; }

    public decimal? HeightMaxMm { get; set; }
    public string? TempRange { get; set; }
    public string? RoHS { get; set; }
    public string? REACHStatus { get; set; }
    public string? DatasheetUrl { get; set; }

    public ICollection<ManufacturerPart> ManufacturerParts { get; set; } = new List<ManufacturerPart>();
    public ICollection<PartDoc> Documents { get; set; } = new List<PartDoc>();
}
