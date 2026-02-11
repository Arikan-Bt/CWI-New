using CWI.Domain.Common;

namespace CWI.Domain.Entities.Lookups;

/// <summary>
/// Dil tanımı entity'si (eski: cdAppLanguage)
/// </summary>
public class Language : BaseEntity, ISoftDeletable
{
    /// <summary>
    /// Dil kodu (örn: tr, en, de)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Dil adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Yerel dil adı (örn: Türkçe, English)
    /// </summary>
    public string? NativeName { get; set; }
    
    /// <summary>
    /// Varsayılan dil mi?
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
