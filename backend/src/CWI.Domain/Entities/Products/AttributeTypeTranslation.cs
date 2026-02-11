using CWI.Domain.Common;
using CWI.Domain.Entities.Lookups;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Özellik tipi çevirisi entity'si (eski: cdProductAttributeTypeDescription)
/// </summary>
public class AttributeTypeTranslation : BaseEntity
{
    /// <summary>
    /// Özellik tipi Id (FK)
    /// </summary>
    public int AttributeTypeId { get; set; }
    
    /// <summary>
    /// Dil Id (FK)
    /// </summary>
    public int LanguageId { get; set; }
    
    /// <summary>
    /// Çevrilmiş özellik tipi adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili özellik tipi
    /// </summary>
    public virtual AttributeType AttributeType { get; set; } = null!;
    
    /// <summary>
    /// İlişkili dil
    /// </summary>
    public virtual Language Language { get; set; } = null!;
}
