using CWI.Domain.Common;
using CWI.Domain.Enums;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Marka entity'si (eski: cdBrand)
/// </summary>
public class Brand : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Proje tipi (CWI veya AWC)
    /// </summary>
    public ProjectType ProjectType { get; set; }
    
    /// <summary>
    /// Marka kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Marka adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Logo URL
    /// </summary>
    public string? LogoUrl { get; set; }
    
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
    /// Markaya ait ürünler
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
