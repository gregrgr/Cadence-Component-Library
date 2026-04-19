namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class QualityReportSection
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public List<QualityReportItem> Items { get; set; } = [];

    public int Count => Items.Count;
}
