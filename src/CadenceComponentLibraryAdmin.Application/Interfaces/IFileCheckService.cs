using CadenceComponentLibraryAdmin.Application.DTOs;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface IFileCheckService
{
    bool FileExists(string? path);
    Task<FileCheckSummary> CheckReleasePartsAsync();
}
