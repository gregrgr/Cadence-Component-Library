namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class FileCheckIssue
{
    public string FileType { get; set; } = null!;
    public string OwnerType { get; set; } = null!;
    public string OwnerKey { get; set; } = null!;
    public string? Path { get; set; }
    public string Status { get; set; } = null!;
}
