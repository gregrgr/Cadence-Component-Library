namespace CadenceComponentLibraryAdmin.Domain.Enums;

public enum ExternalImportStatus
{
    Imported = 0,
    DuplicateFound = 1,
    CandidateCreated = 2,
    Rejected = 3,
    Error = 4
}
