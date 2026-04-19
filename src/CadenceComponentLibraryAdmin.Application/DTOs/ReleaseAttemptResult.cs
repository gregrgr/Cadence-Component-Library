namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class ReleaseAttemptResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = [];
}
