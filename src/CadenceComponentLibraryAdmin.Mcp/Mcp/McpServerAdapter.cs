namespace CadenceComponentLibraryAdmin.Mcp.Mcp;

public interface IMcpServerAdapter
{
    Task RunAsync(CancellationToken cancellationToken);
}
