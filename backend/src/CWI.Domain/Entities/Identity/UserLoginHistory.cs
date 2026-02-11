using CWI.Domain.Common;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Kullanıcı giriş geçmişi entity'si (eski: prUserLogin)
/// </summary>
public class UserLoginHistory : BaseLongEntity
{
    /// <summary>
    /// Kullanıcı Id (FK)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Giriş tarihi
    /// </summary>
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Çıkış tarihi
    /// </summary>
    public DateTime? LogoutAt { get; set; }
    
    /// <summary>
    /// IP adresi
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User Agent
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Başarılı mı?
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Başarısızlık nedeni
    /// </summary>
    public string? FailureReason { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili kullanıcı
    /// </summary>
    public virtual User User { get; set; } = null!;
}
