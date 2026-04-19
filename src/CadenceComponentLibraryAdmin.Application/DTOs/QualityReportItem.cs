namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class QualityReportItem
{
    public string PrimaryKey { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Detail { get; set; }
}
