using CadenceComponentLibraryAdmin.Domain.Common;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class SymbolFamily : BaseEntity
{
    public string SymbolFamilyCode { get; set; } = null!;
    public string SymbolName { get; set; } = null!;
    public string OlbPath { get; set; } = null!;
    public string PartClass { get; set; } = null!;
    public string? GateStyle { get; set; }
    public string? PinMapHash { get; set; }
    public bool IsActive { get; set; }
}
