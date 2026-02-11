using CWI.Domain.Common;
using CWI.Domain.Entities.Lookups;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Renk çevirisi entity'si (eski: cdColorDescription)
/// </summary>
public class ColorTranslation : BaseEntity
{
    /// <summary>
    /// Renk Id (FK)
    /// </summary>
    public int ColorId { get; set; }
    
    /// <summary>
    /// Dil Id (FK)
    /// </summary>
    public int LanguageId { get; set; }
    
    /// <summary>
    /// Çevrilmiş renk adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili renk
    /// </summary>
    public virtual Color Color { get; set; } = null!;
    
    /// <summary>
    /// İlişkili dil
    /// </summary>
    public virtual Language Language { get; set; } = null!;
}
