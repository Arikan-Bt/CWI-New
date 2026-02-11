using CWI.Domain.Common;

namespace CWI.Domain.Entities.Customers;

/// <summary>
/// Müşteri iskonto tanımı entity'si (eski: AWC_MusteriIskonto)
/// </summary>
public class CustomerDiscount : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Müşteri Id (FK)
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// İskonto adı/açıklaması
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// İskonto yüzdesi (0-100 arası)
    /// </summary>
    public decimal DiscountPercentage { get; set; }
    
    /// <summary>
    /// Minimum sipariş tutarı (opsiyonel)
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }
    
    /// <summary>
    /// Geçerlilik başlangıç tarihi
    /// </summary>
    public DateTime ValidFrom { get; set; }
    
    /// <summary>
    /// Geçerlilik bitiş tarihi (null ise süresiz)
    /// </summary>
    public DateTime? ValidTo { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili müşteri
    /// </summary>
    public virtual Customer? Customer { get; set; }
}
