using CWI.Domain.Common;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün özelliği entity'si (eski: cdProductAttribute)
/// </summary>
public class ProductAttribute : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Özellik kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Özellik adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Özellik tipi Id (FK)
    /// </summary>
    public int AttributeTypeId { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili özellik tipi
    /// </summary>
    public virtual AttributeType AttributeType { get; set; } = null!;
    
    /// <summary>
    /// Özellik çevirileri
    /// </summary>
    public virtual ICollection<AttributeTranslation> Translations { get; set; } = new List<AttributeTranslation>();
}
