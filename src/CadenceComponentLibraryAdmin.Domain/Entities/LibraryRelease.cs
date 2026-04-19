using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class LibraryRelease : BaseEntity
{
    public string ReleaseName { get; set; } = null!;
    public DateTime ReleaseDate { get; set; }
    public string ReleasedBy { get; set; } = null!;
    public string? ReleaseNote { get; set; }
    public int? PartCount { get; set; }
    public int? FootprintCount { get; set; }
    public int? SymbolCount { get; set; }
    public ReleaseStatus Status { get; set; }
}
