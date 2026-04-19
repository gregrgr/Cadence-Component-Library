using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class ManufacturerPart : BaseEntity
{
    public string CompanyPN { get; set; } = null!;
    public CompanyPart CompanyPart { get; set; } = null!;

    public string Manufacturer { get; set; } = null!;
    public string ManufacturerPN { get; set; } = null!;

    public string? MfgDescription { get; set; }
    public string? PackageCodeRaw { get; set; }
    public string? SourceProvider { get; set; }

    public LifecycleStatus LifecycleStatus { get; set; }
    public bool IsApproved { get; set; }
    public bool IsPreferred { get; set; }

    public string? ParamJson { get; set; }

    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public ICollection<SupplierOffer> SupplierOffers { get; set; } = new List<SupplierOffer>();
}
