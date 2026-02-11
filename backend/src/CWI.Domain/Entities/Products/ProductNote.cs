using CWI.Domain.Common;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün notu entity'si (eski: cdStockNote)
/// </summary>
public class ProductNote : AuditableEntity
{
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Not içeriği
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Not tipi (örn: Stok, Satış, Bilgi)
    /// </summary>
    public string? NoteType { get; set; }
    
    /// <summary>
    /// Oluşturan kullanıcı
    /// </summary>
    public string? CreatedByUsername { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
