namespace CadenceComponentLibraryAdmin.Domain.Enums;

public enum CandidateStatus
{
    NewFromWeb = 0,
    DuplicateFound = 1,
    PackageMatched = 2,
    PendingSymbol = 3,
    PendingFootprint = 4,
    PendingSupplyCheck = 5,
    PendingReview = 6,
    Approved = 7,
    Rejected = 8
}
