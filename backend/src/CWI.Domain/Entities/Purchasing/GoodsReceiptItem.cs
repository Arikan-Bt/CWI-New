using CWI.Domain.Common;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Purchasing;

/// <summary>
/// Mal alım fişi kalemi entity'si (eski: cdPurchaseLine)
/// </summary>
public class GoodsReceiptItem : BaseLongEntity
{
    /// <summary>
    /// Mal alım fişi Id (FK)
    /// </summary>
    public long GoodsReceiptId { get; set; }
    
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Depo Id (FK)
    /// </summary>
    public int WarehouseId { get; set; }
    
    /// <summary>
    /// Miktar
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Satır toplamı
    /// </summary>
    public decimal LineTotal { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili mal alım fişi
    /// </summary>
    public virtual GoodsReceipt GoodsReceipt { get; set; } = null!;
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// İlişkili depo
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
}
