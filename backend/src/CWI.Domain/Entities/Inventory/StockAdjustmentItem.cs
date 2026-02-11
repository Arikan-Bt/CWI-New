using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Inventory;

/// <summary>
/// Stok düzenleme kalem detayı
/// </summary>
public class StockAdjustmentItem : BaseLongEntity
{
    /// <summary>
    /// Stok düzenleme ana kayıt Id
    /// </summary>
    public long StockAdjustmentId { get; set; }
    
    /// <summary>
    /// Ürün Id
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Eski miktar
    /// </summary>
    public int OldQuantity { get; set; }
    
    /// <summary>
    /// Yeni miktar (sayılan miktar)
    /// </summary>
    public int NewQuantity { get; set; }
    
    /// <summary>
    /// Fark (New - Old)
    /// </summary>
    public int Difference => NewQuantity - OldQuantity;
    
    // New Fields
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? ShelfNumber { get; set; }
    public string? PackList { get; set; }
    public string? ReceivingNumber { get; set; }
    public int? WarehouseId { get; set; }
    public string? SupplierName { get; set; }

    // Navigation Properties
    public virtual StockAdjustment StockAdjustment { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
