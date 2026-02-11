using CWI.Domain.Common;
using CWI.Domain.Entities.Inventory;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün entity'si (eski: cdItem + cdItemPreOrder birleşimi)
/// </summary>
public class Product : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Stok kodu (SKU)
    /// </summary>
    public string Sku { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Renk Id (FK)
    /// </summary>
    public int? ColorId { get; set; }
    
    /// <summary>
    /// Marka Id (FK)
    /// </summary>
    public int? BrandId { get; set; }
    
    /// <summary>
    /// Kategori Id (FK)
    /// </summary>
    public int? CategoryId { get; set; }
    
    /// <summary>
    /// Alt kategori Id (FK)
    /// </summary>
    public int? SubCategoryId { get; set; }
    
    /// <summary>
    /// Ek özellikler (JSON formatında)
    /// </summary>
    public string? Attributes { get; set; }
    
    /// <summary>
    /// Ön sipariş miktarı
    /// </summary>
    public int? PreOrderQuantity { get; set; }
    
    /// <summary>
    /// Ön sipariş ürünü mü?
    /// </summary>
    public bool IsPreOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili marka
    /// </summary>
    public virtual Brand? Brand { get; set; }
    
    /// <summary>
    /// İlişkili renk
    /// </summary>
    public virtual Color? Color { get; set; }
    
    /// <summary>
    /// İlişkili kategori
    /// </summary>
    public virtual AttributeType? Category { get; set; }
    
    /// <summary>
    /// İlişkili alt kategori
    /// </summary>
    public virtual AttributeType? SubCategory { get; set; }
    
    /// <summary>
    /// Ürün çevirileri
    /// </summary>
    public virtual ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();
    
    /// <summary>
    /// Ürün fiyatları
    /// </summary>
    public virtual ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
    
    /// <summary>
    /// Ürün görselleri
    /// </summary>
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    
    /// <summary>
    /// Ürün notları
    /// </summary>
    public virtual ICollection<ProductNote> Notes { get; set; } = new List<ProductNote>();
    
    /// <summary>
    /// Ürün stok bilgileri
    /// </summary>
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
