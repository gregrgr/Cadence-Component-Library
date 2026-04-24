namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class CadenceAutomationOptions
{
    public string JobRoot { get; set; } = "storage/jobs";
    public string CaptureQueuePath { get; set; } = "storage/jobs/capture";
    public string AllegroQueuePath { get; set; } = "storage/jobs/allegro";
    public string LibraryRoot { get; set; } = "library/Cadence";
}
