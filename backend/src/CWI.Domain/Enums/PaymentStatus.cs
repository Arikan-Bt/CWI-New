namespace CWI.Domain.Enums;

/// <summary>
/// Ödeme durumu enum'ı
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Beklemede
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Tamamlandı
    /// </summary>
    Completed = 1,
    
    /// <summary>
    /// Onaylandı
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Başarısız
    /// </summary>
    Failed = 2,
    
    /// <summary>
    /// İade edildi
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected = 4
}
