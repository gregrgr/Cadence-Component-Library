using System.ComponentModel.DataAnnotations;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class AdminRoleListItemViewModel
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public bool IsSystemRole { get; init; }
    public IReadOnlyList<string> Users { get; init; } = [];
}

public sealed class AdminRolesIndexViewModel
{
    public IReadOnlyList<AdminRoleListItemViewModel> Roles { get; init; } = [];
}

public sealed class AdminRoleCreateViewModel
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;
}

public sealed class AdminRoleEditViewModel
{
    public required string Id { get; init; }
    public bool IsSystemRole { get; init; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<string> AssignedUsers { get; init; } = [];
}
