using CWI.Domain.Common;
using CWI.Domain.Enums;

namespace CWI.Domain.Entities.Customers;

/// <summary>
/// Müşteri cari hareket entity'si (eski: cdCurrAccBalance)
/// </summary>
public class CustomerTransaction : BaseLongEntity
{
    /// <summary>
    /// Müşteri Id (FK)
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Hareket tipi
    /// </summary>
    public TransactionType TransactionType { get; set; }
    
    /// <summary>
    /// Hareket tarihi
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>
    /// Referans numarası
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Hareket açıklaması
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Belge tipi
    /// </summary>
    public string? DocumentType { get; set; }
    
    /// <summary>
    /// Uygulama referansı
    /// </summary>
    public string? ApplicationReference { get; set; }
    
    /// <summary>
    /// Borç tutarı
    /// </summary>
    public decimal DebitAmount { get; set; }
    
    /// <summary>
    /// Alacak tutarı
    /// </summary>
    public decimal CreditAmount { get; set; }
    
    /// <summary>
    /// Bakiye
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili müşteri
    /// </summary>
    public virtual Customer? Customer { get; set; }
}
