using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Payments;

namespace CWI.Domain.Entities.Purchasing;

/// <summary>
/// Tedarikçi faturası entity'si (eski: cdVendorInvoice)
/// </summary>
public class VendorInvoice : AuditableEntity
{
    /// <summary>
    /// Tedarikçi Id (FK)
    /// </summary>
    public int VendorId { get; set; }
    
    /// <summary>
    /// Fatura numarası
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Fatura tarihi
    /// </summary>
    public DateTime InvoicedAt { get; set; }
    
    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Para birimi Id (FK)
    /// </summary>
    public int CurrencyId { get; set; }
    
    /// <summary>
    /// Açıklama
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Fatura dosya yolu
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// Vade tarihi
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Ödendi mi?
    /// </summary>
    public bool IsPaid { get; set; }
    
    /// <summary>
    /// Ödenen tutar
    /// </summary>
    public decimal PaidAmount { get; set; }
    
    /// <summary>
    /// Kalan bakiye (computed)
    /// </summary>
    public decimal Balance => TotalAmount - PaidAmount;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili tedarikçi
    /// </summary>
    public virtual Customer Vendor { get; set; } = null!;
    
    /// <summary>
    /// İlişkili para birimi
    /// </summary>
    public virtual Currency Currency { get; set; } = null!;
    
    /// <summary>
    /// Tedarikçi ödemeleri
    /// </summary>
    public virtual ICollection<VendorPayment> Payments { get; set; } = new List<VendorPayment>();
}
