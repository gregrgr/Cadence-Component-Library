using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class OnlineCandidate : BaseEntity
{
    public string SourceProvider { get; set; } = null!;
    public string Manufacturer { get; set; } = null!;
    public string ManufacturerPN { get; set; } = null!;
    public string? Description { get; set; }
    public string? RawPackageName { get; set; }
    public string? MountType { get; set; }
    public int? LeadCount { get; set; }
    public decimal? PitchMm { get; set; }
    public decimal? BodyLmm { get; set; }
    public decimal? BodyWmm { get; set; }
    public decimal? EPLmm { get; set; }
    public decimal? EPWmm { get; set; }
    public string? DatasheetUrl { get; set; }
    public string? RoHS { get; set; }
    public LifecycleStatus LifecycleStatus { get; set; }
    public bool SymbolDownloaded { get; set; }
    public bool FootprintDownloaded { get; set; }
    public bool StepDownloaded { get; set; }
    public CandidateStatus CandidateStatus { get; set; }
    public string? ImportNote { get; set; }
}
