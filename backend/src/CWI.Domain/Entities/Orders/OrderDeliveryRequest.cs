using CWI.Domain.Common;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş teslimat talebi entity'si (eski: trShopCartAddition)
/// </summary>
public class OrderDeliveryRequest : BaseLongEntity
{
    /// <summary>
    /// Sipariş Id (FK)
    /// </summary>
    public long OrderId { get; set; }
    
    /// <summary>
    /// Talep edilen teslimat tarihi
    /// </summary>
    public DateTime? RequestedDeliveryDate { get; set; }
    
    /// <summary>
    /// Teslimat notları
    /// </summary>
    public string? DeliveryNotes { get; set; }
    
    /// <summary>
    /// Özel talepler
    /// </summary>
    public string? SpecialRequests { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public virtual Order Order { get; set; } = null!;
}
