using CWI.Domain.Common;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş paket içeriği entity'si (eski: trShopCartoonLine)
/// </summary>
public class OrderPackageItem : BaseLongEntity
{
    /// <summary>
    /// Paket Id (FK)
    /// </summary>
    public long OrderPackageId { get; set; }
    
    /// <summary>
    /// Sipariş kalemi Id (FK)
    /// </summary>
    public long OrderItemId { get; set; }
    
    /// <summary>
    /// Miktar
    /// </summary>
    public int Quantity { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili paket
    /// </summary>
    public virtual OrderPackage OrderPackage { get; set; } = null!;
    
    /// <summary>
    /// İlişkili sipariş kalemi
    /// </summary>
    public virtual OrderItem OrderItem { get; set; } = null!;
}
