using CWI.Domain.Common;

namespace CWI.Domain.Entities.Payments;

/// <summary>
/// Ödeme bildirimi entity'si (eski: cdPaymentNotificationLog)
/// </summary>
public class PaymentNotification : BaseLongEntity
{
    /// <summary>
    /// Ödeme Id (FK)
    /// </summary>
    public long PaymentId { get; set; }
    
    /// <summary>
    /// Bildirim tipi (Email, SMS, Push)
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;
    
    /// <summary>
    /// Alıcı (e-posta veya telefon)
    /// </summary>
    public string Recipient { get; set; } = string.Empty;
    
    /// <summary>
    /// Bildirim içeriği
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// Gönderildi mi?
    /// </summary>
    public bool IsSent { get; set; }
    
    /// <summary>
    /// Gönderim tarihi
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili ödeme
    /// </summary>
    public virtual Payment Payment { get; set; } = null!;
}
