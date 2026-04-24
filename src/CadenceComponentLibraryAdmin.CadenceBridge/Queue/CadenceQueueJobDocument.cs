namespace CadenceComponentLibraryAdmin.CadenceBridge.Queue;

public sealed class CadenceQueueJobDocument
{
    public long JobId { get; set; }
    public string QueueFamily { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string OverwritePolicy { get; set; } = null!;
    public long? CandidateId { get; set; }
    public long? AiDatasheetExtractionId { get; set; }
    public string Manufacturer { get; set; } = null!;
    public string ManufacturerPartNumber { get; set; } = null!;
    public string LibraryRoot { get; set; } = null!;
    public string SpecJson { get; set; } = null!;
    public string ResultJsonPath { get; set; } = null!;
    public string RequestedByTool { get; set; } = null!;
    public string RequestedAtUtc { get; set; } = null!;
}
