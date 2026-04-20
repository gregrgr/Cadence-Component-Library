namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class ExternalImportOptions
{
    public string? EasyEdaApiKey { get; set; }
    public string? StorageRoot { get; set; }
    public EasyEdaNlbnOptions EasyEdaNlbn { get; set; } = new();
}

public sealed class EasyEdaNlbnOptions
{
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "https://easyeda.com";
    public string ComponentVersion { get; set; } = "6.4.19.5";
    public string ModulesBaseUrl { get; set; } = "https://modules.easyeda.com";
    public string StepPathPrefix { get; set; } = "qAxj6KHrDKw4blvCG8QJPs7Y";
    public bool DownloadStepByDefault { get; set; }
    public bool DownloadObjByDefault { get; set; }
    public bool GeneratePreviewByDefault { get; set; } = true;
    public int MaxParallelImports { get; set; } = 4;
    public int RequestDelayMs { get; set; } = 250;
}
