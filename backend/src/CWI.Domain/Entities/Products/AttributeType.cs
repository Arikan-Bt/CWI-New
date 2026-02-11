using CWI.Domain.Common;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Özellik tipi entity'si (eski: cdProductAttributeType)
/// Kategori ve alt kategoriler için kullanılır
/// </summary>
public class AttributeType : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Özellik tipi kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Özellik tipi adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Üst özellik tipi Id (FK) - hiyerarşi için
    /// </summary>
    public int? ParentId { get; set; }
    
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
    /// Üst özellik tipi
    /// </summary>
    public virtual AttributeType? Parent { get; set; }
    
    /// <summary>
    /// Alt özellik tipleri
    /// </summary>
    public virtual ICollection<AttributeType> Children { get; set; } = new List<AttributeType>();
    
    /// <summary>
    /// Özellik tipi çevirileri
    /// </summary>
    public virtual ICollection<AttributeTypeTranslation> Translations { get; set; } = new List<AttributeTypeTranslation>();
    
    /// <summary>
    /// Bu özellik tipine ait özellikler
    /// </summary>
    public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
}
