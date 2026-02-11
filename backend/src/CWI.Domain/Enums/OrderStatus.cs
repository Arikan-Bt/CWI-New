namespace CWI.Domain.Enums;

/// <summary>
/// Sipariş durumu enum'ı
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Taslak (Ön sipariş için)
    /// </summary>
    Draft = -1,
    
    /// <summary>
    /// Beklemede
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Onaylandı
    /// </summary>
    Approved = 1,
    
    /// <summary>
    /// Sevk edildi
    /// </summary>
    Shipped = 2,
    
    /// <summary>
    /// İptal edildi
    /// </summary>
    Canceled = 3,

    /// <summary>
    /// Ön sipariş
    /// </summary>
    PreOrder = 4,

    /// <summary>
    /// Paketlendi ve sevkiyat bekliyor
    /// </summary>
    PackedAndWaitingShipment = 5
}
