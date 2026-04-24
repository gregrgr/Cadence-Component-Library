using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

public sealed class CodexCliRunner : ICodexCliRunner
{
    private readonly CodexCliOptions _options;
    private readonly ILogger<CodexCliRunner> _logger;

    public CodexCliRunner(
        IOptions<AiExtractionOptions> options,
        ILogger<CodexCliRunner> logger)
    {
        _options = options.Value.CodexCli;
        _logger = logger;
    }

    public async Task<CodexCliRunResult> RunAsync(
        CodexCliRunRequest request,
        CancellationToken cancellationToken = default)
    {
        var timeoutSeconds = Math.Clamp(_options.TimeoutSeconds, 10, 1800);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var outputPath = Path.Combine(Path.GetTempPath(), $"cadence-ai-codex-{Guid.NewGuid():N}.json");
        try
        {
            using var process = StartProcess(outputPath);
            await process.StandardInput.WriteAsync(request.Prompt.AsMemory(), timeout.Token);
            process.StandardInput.Close();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(timeout.Token);

            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                throw new TimeoutException($"Codex CLI extraction timed out after {timeoutSeconds} seconds.");
            }

            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            var output = File.Exists(outputPath) ? await File.ReadAllTextAsync(outputPath, cancellationToken) : stdout;

            if (process.ExitCode != 0)
            {
                _logger.LogError("Codex CLI exited with code {ExitCode}.", process.ExitCode);
            }

            return new CodexCliRunResult(process.ExitCode, output, stderr);
        }
        finally
        {
            TryDelete(outputPath);
        }
    }

    private Process StartProcess(string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = string.IsNullOrWhiteSpace(_options.Command) ? "codex" : _options.Command,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var workingDirectory = string.IsNullOrWhiteSpace(_options.WorkingDirectory)
            ? Directory.GetCurrentDirectory()
            : _options.WorkingDirectory;
        startInfo.WorkingDirectory = workingDirectory;

        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add("--skip-git-repo-check");
        startInfo.ArgumentList.Add("--sandbox");
        startInfo.ArgumentList.Add(string.IsNullOrWhiteSpace(_options.Sandbox) ? "read-only" : _options.Sandbox);
        startInfo.ArgumentList.Add("--color");
        startInfo.ArgumentList.Add("never");
        startInfo.ArgumentList.Add("--output-last-message");
        startInfo.ArgumentList.Add(outputPath);

        if (_options.Ephemeral)
        {
            startInfo.ArgumentList.Add("--ephemeral");
        }

        if (!string.IsNullOrWhiteSpace(_options.Model))
        {
            startInfo.ArgumentList.Add("--model");
            startInfo.ArgumentList.Add(_options.Model);
        }

        if (!string.IsNullOrWhiteSpace(_options.Profile))
        {
            startInfo.ArgumentList.Add("--profile");
            startInfo.ArgumentList.Add(_options.Profile);
        }

        startInfo.ArgumentList.Add("-");

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start Codex CLI process.");
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
