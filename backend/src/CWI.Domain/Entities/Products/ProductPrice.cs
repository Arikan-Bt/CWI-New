using CWI.Domain.Common;
using CWI.Domain.Entities.Payments;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün fiyatı entity'si (eski: PriceList + PriceListEUR birleşimi)
/// </summary>
public class ProductPrice : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Marka Id (FK) - belirli bir markaya özel fiyat için
    /// </summary>
    public int? BrandId { get; set; }
    
    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Para birimi Id (FK)
    /// </summary>
    public int CurrencyId { get; set; }
    
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
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// İlişkili marka
    /// </summary>
    public virtual Brand? Brand { get; set; }
    
    /// <summary>
    /// İlişkili para birimi
    /// </summary>
    public virtual Currency Currency { get; set; } = null!;
}
