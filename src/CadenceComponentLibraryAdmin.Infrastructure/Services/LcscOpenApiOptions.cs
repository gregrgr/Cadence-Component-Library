namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class LcscOpenApiOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://ips.lcsc.com";
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string Currency { get; set; } = "USD";
    public bool AllowAssetDownloads { get; set; }
}
