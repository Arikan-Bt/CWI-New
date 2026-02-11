using CWI.Domain.Common;
using CWI.Domain.Entities.Lookups;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün çevirisi entity'si (eski: cdItemDesc)
/// </summary>
public class ProductTranslation : BaseEntity
{
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Dil Id (FK)
    /// </summary>
    public int LanguageId { get; set; }
    
    /// <summary>
    /// Çevrilmiş ürün adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Çevrilmiş ürün açıklaması
    /// </summary>
    public string? Description { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// İlişkili dil
    /// </summary>
    public virtual Language Language { get; set; } = null!;
}
