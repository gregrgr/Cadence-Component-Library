namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class FileCheckSummary
{
    public int TotalChecked { get; set; }
    public int MissingCount { get; set; }
    public List<FileCheckIssue> Issues { get; set; } = [];
}
