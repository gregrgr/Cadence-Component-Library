using Microsoft.Extensions.Logging;

namespace CadenceComponentLibraryAdmin.Mcp.Mcp;

public sealed class PlaceholderMcpServerAdapter : IMcpServerAdapter
{
    private readonly ILogger<PlaceholderMcpServerAdapter> _logger;

    public PlaceholderMcpServerAdapter(ILogger<PlaceholderMcpServerAdapter> logger)
    {
        _logger = logger;
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting placeholder MCP server adapter.");
        _logger.LogInformation("Official Model Context Protocol C# SDK is not wired into this repository yet.");
        _logger.LogInformation("This placeholder host keeps the MCP project buildable while tool logic lives in IMcpLibraryWorkflowService.");
        _logger.LogInformation("Available tool names: library_get_candidate, library_search_duplicate, datasheet_create_extraction_draft, datasheet_submit_for_review, datasheet_approve_for_build, capture_enqueue_symbol_job, allegro_enqueue_footprint_job, cadence_get_job_status, verification_get_report.");
        return Task.CompletedTask;
    }
}
