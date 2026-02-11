using CWI.Domain.Common;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Renk entity'si (eski: cdColor)
/// </summary>
public class Color : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Renk kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Renk adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Hex renk kodu (örn: #FF5733)
    /// </summary>
    public string? HexCode { get; set; }
    
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
    /// Bu renge ait ürünler
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    
    /// <summary>
    /// Renk çevirileri
    /// </summary>
    public virtual ICollection<ColorTranslation> Translations { get; set; } = new List<ColorTranslation>();
}
