using CWI.Domain.Common;
using CWI.Domain.Entities.Products;
using CWI.Domain.Enums;

namespace CWI.Domain.Entities.Inventory;

public class StockMovement : AuditableLongEntity
{
    public int ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public StockMovementType MovementType { get; set; }

    public int QuantityDeltaOnHand { get; set; }
    public int QuantityDeltaReserved { get; set; }

    public int BeforeOnHand { get; set; }
    public int AfterOnHand { get; set; }
    public int BeforeReserved { get; set; }
    public int AfterReserved { get; set; }

    public string SourceDocumentType { get; set; } = string.Empty;
    public long? SourceDocumentId { get; set; }
    public string? ReferenceNo { get; set; }

    public string? ShelfNumber { get; set; }
    public string? PackList { get; set; }
    public string? SupplierName { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Description { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse? Warehouse { get; set; }
}
