using CWI.Domain.Common;

namespace CWI.Domain.Entities.Customers;

/// <summary>
/// Müşteri iletişim bilgisi entity'si (eski: cdCrm)
/// </summary>
public class CustomerContact : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Müşteri Id (FK)
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// İletişim adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// İletişim pozisyonu/unvanı
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Notlar
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Birincil iletişim mi?
    /// </summary>
    public bool IsPrimary { get; set; }
    
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
