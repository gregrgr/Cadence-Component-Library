using CadenceComponentLibraryAdmin.Domain.Entities;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class ApprovalQueueViewModel
{
    public IReadOnlyList<OnlineCandidate> PendingOnlineCandidates { get; init; } = [];
    public IReadOnlyList<CompanyPart> PendingCompanyParts { get; init; } = [];
    public IReadOnlyList<FootprintVariant> ReviewingFootprints { get; init; } = [];
}
