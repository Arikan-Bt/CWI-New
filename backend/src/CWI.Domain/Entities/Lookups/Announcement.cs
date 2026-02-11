using CWI.Domain.Common;

namespace CWI.Domain.Entities.Lookups;

/// <summary>
/// Duyuru entity'si (eski: cdCompanyNews)
/// </summary>
public class Announcement : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Duyuru başlığı
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Duyuru içeriği
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Özet (kısa açıklama)
    /// </summary>
    public string? Summary { get; set; }
    
    /// <summary>
    /// Görsel URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Yayın başlangıç tarihi
    /// </summary>
    public DateTime PublishFrom { get; set; }
    
    /// <summary>
    /// Yayın bitiş tarihi
    /// </summary>
    public DateTime? PublishTo { get; set; }
    
    /// <summary>
    /// Önemli duyuru mu?
    /// </summary>
    public bool IsImportant { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
