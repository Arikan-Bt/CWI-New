using CWI.Domain.Common;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün görseli entity'si (eski: cdImageUrl)
/// </summary>
public class ProductImage : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Görsel URL'i
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Görsel başlığı
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Alt metin (SEO için)
    /// </summary>
    public string? AltText { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Ana görsel mi?
    /// </summary>
    public bool IsPrimary { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
