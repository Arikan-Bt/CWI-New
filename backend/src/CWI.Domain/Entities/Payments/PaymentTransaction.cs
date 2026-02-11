using CWI.Domain.Common;

namespace CWI.Domain.Entities.Payments;

/// <summary>
/// Ödeme işlemi (banka log) entity'si (eski: cdTransactionLog)
/// </summary>
public class PaymentTransaction : BaseLongEntity
{
    /// <summary>
    /// Ödeme Id (FK)
    /// </summary>
    public long PaymentId { get; set; }
    
    /// <summary>
    /// İşlem referans numarası
    /// </summary>
    public string? TransactionReference { get; set; }
    
    /// <summary>
    /// Banka yanıt kodu
    /// </summary>
    public string? ResponseCode { get; set; }
    
    /// <summary>
    /// Banka yanıt mesajı
    /// </summary>
    public string? ResponseMessage { get; set; }
    
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// İşlem tutarı
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// İşlem tarihi
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>
    /// Ham istek verisi
    /// </summary>
    public string? RequestData { get; set; }
    
    /// <summary>
    /// Ham yanıt verisi
    /// </summary>
    public string? ResponseData { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili ödeme
    /// </summary>
    public virtual Payment Payment { get; set; } = null!;
}
