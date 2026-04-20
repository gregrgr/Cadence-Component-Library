using System.ComponentModel.DataAnnotations;

namespace CadenceComponentLibraryAdmin.Web.Models;

public sealed class AdminUserListItemViewModel
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public required string UserName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public bool EmailConfirmed { get; init; }
    public bool LockoutEnabled { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public int AccessFailedCount { get; init; }
}

public sealed class AdminUsersIndexViewModel
{
    public string? Search { get; init; }
    public IReadOnlyList<AdminUserListItemViewModel> Users { get; init; } = [];
}

public sealed class AdminUserCreateViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IReadOnlyList<string> AvailableRoles { get; set; } = [];
    public List<string> SelectedRoles { get; set; } = [];
}

public sealed class AdminUserEditViewModel
{
    public required string Id { get; init; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public int AccessFailedCount { get; init; }
    public IReadOnlyList<string> AvailableRoles { get; set; } = [];
    public List<string> SelectedRoles { get; set; } = [];
}

public sealed class AdminResetPasswordViewModel
{
    public required string Id { get; init; }
    public required string Email { get; init; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
