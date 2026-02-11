using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Enums;

namespace CWI.Domain.Entities.Payments;

/// <summary>
/// Ödeme entity'si (eski: cdPayment)
/// </summary>
public class Payment : AuditableLongEntity, IUserAuditableEntity
{
    /// <summary>
    /// Müşteri Id (FK)
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Sipariş Id (FK) - opsiyonel
    /// </summary>
    public long? OrderId { get; set; }
    
    /// <summary>
    /// Ödeme tutarı
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Para birimi Id (FK)
    /// </summary>
    public int CurrencyId { get; set; }
    
    /// <summary>
    /// Makbuz numarası
    /// </summary>
    public string? ReceiptNumber { get; set; }
    
    /// <summary>
    /// Ödeme tarihi
    /// </summary>
    public DateTime PaidAt { get; set; }
    
    /// <summary>
    /// Ödeme yöntemi Id (FK)
    /// </summary>
    public int PaymentMethodId { get; set; }
    
    /// <summary>
    /// Ödeme durumu
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    /// <summary>
    /// Makbuz dosya yolu
    /// </summary>
    public string? ReceiptFilePath { get; set; }
    
    /// <summary>
    /// Notlar
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Oluşturan kullanıcı adı
    /// </summary>
    public string? CreatedByUsername { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili müşteri
    /// </summary>
    public virtual Customer? Customer { get; set; }
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public virtual Order? Order { get; set; }
    
    /// <summary>
    /// İlişkili para birimi
    /// </summary>
    public virtual Currency Currency { get; set; } = null!;
    
    /// <summary>
    /// İlişkili ödeme yöntemi
    /// </summary>
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    
    /// <summary>
    /// Ödeme işlemleri
    /// </summary>
    public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    
    /// <summary>
    /// Ödeme bildirimleri
    /// </summary>
    public virtual ICollection<PaymentNotification> Notifications { get; set; } = new List<PaymentNotification>();
}
