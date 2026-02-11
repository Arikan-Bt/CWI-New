using CWI.Domain.Common;

namespace CWI.Domain.Entities.Payments;

/// <summary>
/// Ödeme yöntemi entity'si (eski: cdPaymentMethod)
/// </summary>
public class PaymentMethod : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Ödeme yöntemi kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Ödeme yöntemi adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Açıklama
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// Bu yöntemle yapılan ödemeler
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
