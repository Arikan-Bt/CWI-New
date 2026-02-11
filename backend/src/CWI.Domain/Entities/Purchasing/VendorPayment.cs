using CWI.Domain.Common;
using CWI.Domain.Entities.Payments;

namespace CWI.Domain.Entities.Purchasing;

/// <summary>
/// Tedarikçi ödemesi entity'si (eski: cdPaymentVendor)
/// </summary>
public class VendorPayment : AuditableLongEntity
{
    /// <summary>
    /// Tedarikçi Id (FK)
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Tedarikçi faturası Id (FK) - opsiyonel
    /// </summary>
    public int? VendorInvoiceId { get; set; }
    
    /// <summary>
    /// Ödeme tutarı
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Ödeme dosyası yolu (makbuz vs)
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// Para birimi Id (FK)
    /// </summary>
    public int CurrencyId { get; set; }
    
    /// <summary>
    /// Ödeme tarihi
    /// </summary>
    public DateTime PaidAt { get; set; }
    
    /// <summary>
    /// Referans numarası
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Açıklama
    /// </summary>
    public string? Description { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili tedarikçi
    /// </summary>
    public virtual Customers.Customer Vendor { get; set; } = null!;

    /// <summary>
    /// İlişkili tedarikçi faturası
    /// </summary>
    public virtual VendorInvoice? VendorInvoice { get; set; }
    
    /// <summary>
    /// İlişkili para birimi
    /// </summary>
    public virtual Currency Currency { get; set; } = null!;
}
