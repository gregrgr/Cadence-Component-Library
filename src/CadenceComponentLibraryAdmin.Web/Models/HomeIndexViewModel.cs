namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class HomeIndexViewModel
{
    public string EnvironmentName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string AppDataRoot { get; set; } = string.Empty;
    public string LibraryRoot { get; set; } = string.Empty;
    public string LogRoot { get; set; } = string.Empty;
    public IReadOnlyCollection<DashboardCardViewModel> Cards { get; set; } = [];
}
