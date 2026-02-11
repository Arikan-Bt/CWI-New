using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Inventory;

/// <summary>
/// Stok kalemi entity'si (eski: trWareHouseItems)
/// </summary>
public class InventoryItem : BaseLongEntity
{
    /// <summary>
    /// Depo Id (FK)
    /// </summary>
    public int WarehouseId { get; set; }
    
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Eldeki miktar
    /// </summary>
    public int QuantityOnHand { get; set; }
    
    /// <summary>
    /// Rezerve edilmiş miktar
    /// </summary>
    public int QuantityReserved { get; set; }
    
    /// <summary>
    /// Kullanılabilir miktar (computed)
    /// </summary>
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;
    
    /// <summary>
    /// Yeniden sipariş seviyesi
    /// </summary>
    public int? ReorderLevel { get; set; }
    
    /// <summary>
    /// Raf numarası
    /// </summary>
    public string? ShelfNumber { get; set; }

    /// <summary>
    /// Son sayım tarihi
    /// </summary>
    public DateTime? LastStockTakeAt { get; set; }
    
    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili depo
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
