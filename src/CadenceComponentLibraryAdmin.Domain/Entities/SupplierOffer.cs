using CadenceComponentLibraryAdmin.Domain.Common;

namespace CadenceComponentLibraryAdmin.Domain.Entities;

public class SupplierOffer : BaseEntity
{
    public long ManufacturerPartId { get; set; }
    public ManufacturerPart ManufacturerPart { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public string? SupplierSku { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? Moq { get; set; }
    public int? LeadTimeDays { get; set; }
    public long? StockQty { get; set; }
    public DateTime SnapshotAt { get; set; }
}
