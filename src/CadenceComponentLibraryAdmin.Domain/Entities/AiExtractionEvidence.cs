using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public sealed class AiExtractionEvidence
{
    public long Id { get; set; }
    public long AiDatasheetExtractionId { get; set; }
    public AiDatasheetExtraction AiDatasheetExtraction { get; set; } = null!;
    public string FieldPath { get; set; } = null!;
    public string ValueText { get; set; } = null!;
    public string? Unit { get; set; }
    public int? SourcePage { get; set; }
    public string? SourceTable { get; set; }
    public string? SourceFigure { get; set; }
    public decimal Confidence { get; set; }
    public AiExtractionReviewerDecision ReviewerDecision { get; set; } = AiExtractionReviewerDecision.Pending;
    public string? ReviewerNote { get; set; }
}
