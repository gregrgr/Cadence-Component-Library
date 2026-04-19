using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Application.Interfaces;

public interface ILibraryReleaseService
{
    Task<LibraryReleaseDraftDto> BuildDraftAsync(CancellationToken cancellationToken = default);

    Task<LibraryRelease> CreateDraftAsync(
        string releasedBy,
        string? releaseNote,
        CancellationToken cancellationToken = default);

    Task<ReleaseAttemptResult> ReleaseAsync(
        long id,
        string releasedBy,
        CancellationToken cancellationToken = default);
}
