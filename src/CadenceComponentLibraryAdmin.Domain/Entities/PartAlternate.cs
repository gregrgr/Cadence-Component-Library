using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class PartAlternate : BaseEntity
{
    public string SourceCompanyPN { get; set; } = null!;
    public string TargetCompanyPN { get; set; } = null!;
    public AlternateLevel AltLevel { get; set; }
    public bool SameFootprintYN { get; set; }
    public bool SameSymbolYN { get; set; }
    public bool NeedEEReviewYN { get; set; }
    public bool NeedLayoutReviewYN { get; set; }
    public string? Notes { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
