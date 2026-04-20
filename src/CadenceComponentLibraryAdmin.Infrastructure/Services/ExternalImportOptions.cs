namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class ExternalImportOptions
{
    public string? EasyEdaApiKey { get; set; }
    public bool AllowLegacyApiKeyInDevelopment { get; set; } = true;
    public string? StorageRoot { get; set; }
}
