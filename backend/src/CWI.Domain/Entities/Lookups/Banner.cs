using CWI.Domain.Common;

namespace CWI.Domain.Entities.Lookups;

/// <summary>
/// Banner yönetimi entity'si (eski: cdBannerManagment)
/// </summary>
public class Banner : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Banner başlığı
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Açıklama
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Görsel URL
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Yönlendirme URL'i
    /// </summary>
    public string? LinkUrl { get; set; }
    
    /// <summary>
    /// Konum (örn: Home, Category, Product)
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Yayın başlangıç tarihi
    /// </summary>
    public DateTime PublishFrom { get; set; }
    
    /// <summary>
    /// Yayın bitiş tarihi
    /// </summary>
    public DateTime? PublishTo { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
