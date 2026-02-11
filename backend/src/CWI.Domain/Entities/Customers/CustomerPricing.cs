using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Customers;

/// <summary>
/// Müşteri-Marka fiyat ilişkisi entity'si (eski: AWC_MusteriCurrency)
/// </summary>
public class CustomerPricing : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Müşteri Id (FK)
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Marka Id (FK)
    /// </summary>
    public int BrandId { get; set; }
    
    /// <summary>
    /// Fiyat çarpanı (0-1 arası iskonto, 1+ fiyat artışı)
    /// </summary>
    public decimal PriceMultiplier { get; set; } = 1;
    
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
    
    /// <summary>
    /// İlişkili marka
    /// </summary>
    public virtual Brand Brand { get; set; } = null!;
}
