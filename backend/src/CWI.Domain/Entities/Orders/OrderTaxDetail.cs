using CWI.Domain.Common;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş vergi detayı entity'si (VatPercent ve VatBase alanları için)
/// </summary>
public class OrderTaxDetail : BaseLongEntity
{
    /// <summary>
    /// Sipariş Id (FK)
    /// </summary>
    public long OrderId { get; set; }
    
    /// <summary>
    /// Vergi oranı (%)
    /// </summary>
    public decimal TaxRate { get; set; }
    
    /// <summary>
    /// Vergi matrahı
    /// </summary>
    public decimal TaxableAmount { get; set; }
    
    /// <summary>
    /// Vergi tutarı
    /// </summary>
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Vergi tipi (KDV, ÖTV, vb.)
    /// </summary>
    public string TaxType { get; set; } = "VAT";
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public virtual Order Order { get; set; } = null!;
}
