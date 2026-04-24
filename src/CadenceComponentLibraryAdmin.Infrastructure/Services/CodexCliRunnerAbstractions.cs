namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public interface ICodexCliRunner
{
    Task<CodexCliRunResult> RunAsync(
        CodexCliRunRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record CodexCliRunRequest(string Prompt);

public sealed record CodexCliRunResult(
    int ExitCode,
    string Output,
    string ErrorOutput);
