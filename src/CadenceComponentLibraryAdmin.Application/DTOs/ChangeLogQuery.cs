using CadenceComponentLibraryAdmin.Domain.Enums;

namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class ChangeLogQuery
{
    public string? CompanyPN { get; set; }
    public ChangeType? ChangeType { get; set; }
    public DateTime? ChangedFrom { get; set; }
    public DateTime? ChangedTo { get; set; }
}
