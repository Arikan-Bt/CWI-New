using CWI.Domain.Common;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş ERP senkron durumu entity'si (eski: CaniasOrders)
/// </summary>
public class OrderErpSync : BaseLongEntity
{
    /// <summary>
    /// Sipariş Id (FK)
    /// </summary>
    public long OrderId { get; set; }
    
    /// <summary>
    /// ERP sipariş numarası
    /// </summary>
    public string? ErpOrderNumber { get; set; }
    
    /// <summary>
    /// Senkron edildi mi?
    /// </summary>
    public bool IsSynced { get; set; }
    
    /// <summary>
    /// Son senkron tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
    
    /// <summary>
    /// Senkron hata mesajı
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Senkron deneme sayısı
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// ERP yanıt verisi (JSON)
    /// </summary>
    public string? ErpResponseData { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public virtual Order Order { get; set; } = null!;
}
