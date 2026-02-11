using CWI.Domain.Common;
using CWI.Domain.Entities.Lookups;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün özelliği çevirisi entity'si (eski: cdProductAttributeDescription)
/// </summary>
public class AttributeTranslation : BaseEntity
{
    /// <summary>
    /// Ürün özelliği Id (FK)
    /// </summary>
    public int ProductAttributeId { get; set; }
    
    /// <summary>
    /// Dil Id (FK)
    /// </summary>
    public int LanguageId { get; set; }
    
    /// <summary>
    /// Çevrilmiş özellik adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili ürün özelliği
    /// </summary>
    public virtual ProductAttribute ProductAttribute { get; set; } = null!;
    
    /// <summary>
    /// İlişkili dil
    /// </summary>
    public virtual Language Language { get; set; } = null!;
}
