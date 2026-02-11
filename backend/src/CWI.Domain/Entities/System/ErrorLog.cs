using CWI.Domain.Common;

namespace CWI.Domain.Entities.System;

/// <summary>
/// Hata log entity'si (eski: DB_Errors)
/// </summary>
public class ErrorLog : BaseLongEntity
{
    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Stack trace
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// İç hata mesajı
    /// </summary>
    public string? InnerException { get; set; }
    
    /// <summary>
    /// Kaynak (sınıf/metod adı)
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Kullanıcı adı
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// IP adresi
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// İstek URL'i
    /// </summary>
    public string? RequestUrl { get; set; }
    
    /// <summary>
    /// HTTP metodu
    /// </summary>
    public string? HttpMethod { get; set; }
    
    /// <summary>
    /// İstek gövdesi
    /// </summary>
    public string? RequestBody { get; set; }
    
    /// <summary>
    /// Hata tarihi
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Çözüldü mü?
    /// </summary>
    public bool IsResolved { get; set; }
    
    /// <summary>
    /// Çözüm notu
    /// </summary>
    public string? ResolutionNote { get; set; }
}
