using CWI.Domain.Common;

namespace CWI.Domain.Entities.System;

/// <summary>
/// Import işlemi entity'si (eski: XMLImportData)
/// </summary>
public class ImportJob : AuditableLongEntity
{
    /// <summary>
    /// İş adı
    /// </summary>
    public string JobName { get; set; } = string.Empty;
    
    /// <summary>
    /// Kaynak tipi (XML, CSV, Excel, API)
    /// </summary>
    public string SourceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Kaynak yolu/URL
    /// </summary>
    public string? SourcePath { get; set; }
    
    /// <summary>
    /// Durum (Pending, Running, Completed, Failed)
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    /// <summary>
    /// Başlangıç tarihi
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Bitiş tarihi
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Toplam kayıt sayısı
    /// </summary>
    public int TotalRecords { get; set; }
    
    /// <summary>
    /// İşlenen kayıt sayısı
    /// </summary>
    public int ProcessedRecords { get; set; }
    
    /// <summary>
    /// Başarılı kayıt sayısı
    /// </summary>
    public int SuccessfulRecords { get; set; }
    
    /// <summary>
    /// Başarısız kayıt sayısı
    /// </summary>
    public int FailedRecords { get; set; }
    
    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Hata detayları (JSON formatında)
    /// </summary>
    public string? ErrorDetails { get; set; }
    
    /// <summary>
    /// İşlemi başlatan kullanıcı
    /// </summary>
    public string? InitiatedByUsername { get; set; }
}
