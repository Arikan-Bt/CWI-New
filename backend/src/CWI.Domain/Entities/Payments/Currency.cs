using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Payments;

/// <summary>
/// Para birimi entity'si (eski: cdCurrency)
/// </summary>
public class Currency : BaseEntity, ISoftDeletable
{
    /// <summary>
    /// Para birimi kodu (örn: TRY, USD, EUR)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Para birimi adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Para birimi sembolü (örn: ₺, $, €)
    /// </summary>
    public string? Symbol { get; set; }
    
    /// <summary>
    /// Varsayılan para birimi mi?
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// Bu para birimi ile yapılan ödemeler
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    /// <summary>
    /// Bu para birimindeki ürün fiyatları
    /// </summary>
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
}
