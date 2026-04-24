namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class AiExtractionOptions
{
    public string Mode { get; set; } = "Stub";
    public OpenAiCompatibleOptions OpenAI { get; set; } = new();
    public CodexCliOptions CodexCli { get; set; } = new();
}

public sealed class OpenAiCompatibleOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4.1-mini";
    public string? ApiKey { get; set; }
}

public sealed class CodexCliOptions
{
    public bool Enabled { get; set; }
    public string Command { get; set; } = "codex";
    public string? Model { get; set; }
    public string? Profile { get; set; }
    public string Sandbox { get; set; } = "read-only";
    public bool Ephemeral { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 180;
    public string? WorkingDirectory { get; set; }
}
