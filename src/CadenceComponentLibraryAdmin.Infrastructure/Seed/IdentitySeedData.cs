namespace CadenceComponentLibraryAdmin.Infrastructure.Seed;

public static class IdentitySeedData
{
    public const string AdminEmail = "admin@local.test";
    public const string AdminPassword = "Admin@123456";

    public static readonly string[] Roles =
    [
        "Admin",
        "Librarian",
        "EEReviewer",
        "Purchasing",
        "Designer",
        "Viewer"
    ];
}
