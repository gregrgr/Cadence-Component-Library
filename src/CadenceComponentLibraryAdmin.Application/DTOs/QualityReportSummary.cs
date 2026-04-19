namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class QualityReportSummary
{
    public List<QualityReportSection> Sections { get; set; } = [];

    public int TotalFindings => Sections.Sum(x => x.Count);
}
