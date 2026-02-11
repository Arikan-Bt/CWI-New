using CWI.Domain.Common;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş teslimat bilgisi entity'si (eski: trShopCartDetail)
/// </summary>
public class OrderShippingInfo : BaseLongEntity
{
    /// <summary>
    /// Sipariş Id (FK)
    /// </summary>
    public long OrderId { get; set; }
    
    /// <summary>
    /// Teslimat adresi
    /// </summary>
    public string? ShippingAddress { get; set; }
    
    /// <summary>
    /// Ödeme yöntemi
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Sevkiyat koşulları
    /// </summary>
    public string? ShipmentTerms { get; set; }
    
    /// <summary>
    /// Ek iskonto
    /// </summary>
    public decimal AdditionalDiscount { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public virtual Order Order { get; set; } = null!;
}
