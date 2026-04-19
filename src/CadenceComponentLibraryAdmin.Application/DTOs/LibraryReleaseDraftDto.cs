namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class LibraryReleaseDraftDto
{
    public string ReleaseName { get; set; } = null!;
    public DateTime ReleaseDate { get; set; }
    public int PartCount { get; set; }
    public int FootprintCount { get; set; }
    public int SymbolCount { get; set; }
}
