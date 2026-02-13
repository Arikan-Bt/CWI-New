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
    /// Trace kimliÄŸi
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// KullanÄ±cÄ± kimliÄŸi
    /// </summary>
    public int? UserId { get; set; }
    
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
    /// Exception tipi
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// HatanÄ±n oluÅŸtuÄŸu hedef metot/sÄ±nÄ±f
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Query string verisi (JSON)
    /// </summary>
    public string? RequestQuery { get; set; }

    /// <summary>
    /// Route parametreleri (JSON)
    /// </summary>
    public string? RequestRouteValues { get; set; }

    /// <summary>
    /// SeÃ§ili request header verileri (JSON)
    /// </summary>
    public string? RequestHeaders { get; set; }

    /// <summary>
    /// Request content type
    /// </summary>
    public string? RequestContentType { get; set; }

    /// <summary>
    /// Request content length
    /// </summary>
    public long? RequestContentLength { get; set; }
    
    /// <summary>
    /// İstek gövdesi
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// MaskelenmiÅŸ istek gÃ¶vdesi
    /// </summary>
    public string? RequestBodyMasked { get; set; }

    /// <summary>
    /// Hata kodu
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Hata veren parametre
    /// </summary>
    public string? ParameterName { get; set; }

    /// <summary>
    /// Ã‡alÄ±ÅŸma ortamÄ±
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Makine adÄ±
    /// </summary>
    public string? MachineName { get; set; }
    
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

    /// <summary>
    /// Ã‡Ã¶zÃ¼ldÃ¼ tarihi
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Ã‡Ã¶zÃ¼m yapan kullanÄ±cÄ±
    /// </summary>
    public int? ResolvedByUserId { get; set; }
}
