using CWI.Domain.Common;

namespace CWI.Domain.Entities.System;

/// <summary>
/// Uygulama log entity'si (eski: Logs)
/// </summary>
public class ApplicationLog : BaseLongEntity
{
    /// <summary>
    /// Log seviyesi (Debug, Info, Warning, Error, Fatal)
    /// </summary>
    public string Level { get; set; } = string.Empty;
    
    /// <summary>
    /// Log mesajı
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
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
    /// İstek verisi
    /// </summary>
    public string? RequestData { get; set; }
    
    /// <summary>
    /// Ek veri (JSON formatında)
    /// </summary>
    public string? AdditionalData { get; set; }
    
    /// <summary>
    /// Log tarihi
    /// </summary>
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
