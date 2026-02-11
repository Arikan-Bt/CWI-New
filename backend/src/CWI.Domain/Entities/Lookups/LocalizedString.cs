using CWI.Domain.Common;

namespace CWI.Domain.Entities.Lookups;

/// <summary>
/// Yerelleştirilmiş metin entity'si (eski: cdAppFields)
/// </summary>
public class LocalizedString : BaseEntity
{
    /// <summary>
    /// Dil Id (FK)
    /// </summary>
    public int LanguageId { get; set; }
    
    /// <summary>
    /// Anahtar (benzersiz tanımlayıcı)
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Değer (çeviri)
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Modül/Bölüm adı
    /// </summary>
    public string? Module { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili dil
    /// </summary>
    public virtual Language Language { get; set; } = null!;
}
