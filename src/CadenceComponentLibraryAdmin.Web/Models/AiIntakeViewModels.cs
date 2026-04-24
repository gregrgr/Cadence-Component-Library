using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class AiIntakeIndexViewModel
{
    public PagedResult<AiIntakeListItemViewModel> Result { get; init; } = new();
    public AiDatasheetExtractionStatus? Status { get; init; }
}

public sealed class AiIntakeListItemViewModel
{
    public long Id { get; init; }
    public string Manufacturer { get; init; } = null!;
    public string ManufacturerPartNumber { get; init; } = null!;
    public decimal Confidence { get; init; }
    public AiDatasheetExtractionStatus Status { get; init; }
    public long? CandidateId { get; init; }
    public long? ExternalImportId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class AiIntakeCreateViewModel
{
    public long ExternalImportId { get; set; }
    public string? Manufacturer { get; set; }
    public string? ManufacturerPartNumber { get; set; }
    public string? DatasheetUrlOrPath { get; set; }
    public string ExtractionJson { get; set; } = "{}";
    public string SymbolSpecJson { get; set; } = "{}";
    public string FootprintSpecJson { get; set; } = "{}";
}

public sealed class AiIntakeEditViewModel
{
    public long Id { get; set; }
    public string Manufacturer { get; set; } = null!;
    public string ManufacturerPartNumber { get; set; } = null!;
    public string ExtractionJson { get; set; } = "{}";
    public string SymbolSpecJson { get; set; } = "{}";
    public string FootprintSpecJson { get; set; } = "{}";
    public AiDatasheetExtractionStatus Status { get; set; }
}

public sealed class AiIntakeDetailsViewModel
{
    public AiDatasheetExtraction Extraction { get; init; } = null!;
    public ExternalComponentImport? ExternalImport { get; init; }
    public OnlineCandidate? Candidate { get; init; }
    public IReadOnlyList<AiExtractionEvidence> EvidenceItems { get; init; } = [];
    public IReadOnlyList<CadenceBuildJob> Jobs { get; init; } = [];
    public bool CanApproveForBuild { get; init; }
    public bool CanBuildSymbol { get; init; }
    public bool CanBuildFootprint { get; init; }
    public bool BuildActionsEnabled => Extraction.Status == AiDatasheetExtractionStatus.ApprovedForBuild;
}

public sealed class AiIntakeJobsViewModel
{
    public AiDatasheetExtraction Extraction { get; init; } = null!;
    public IReadOnlyList<CadenceBuildJob> Jobs { get; init; } = [];
}

public sealed class AiIntakeVerificationViewModel
{
    public AiDatasheetExtraction Extraction { get; init; } = null!;
    public LibraryVerificationReport? Report { get; init; }
}
