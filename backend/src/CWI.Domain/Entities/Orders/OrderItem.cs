using CWI.Domain.Common;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş kalemi entity'si (eski: trShopCartLine)
/// </summary>
public class OrderItem : AuditableLongEntity, IUserAuditableEntity
{
    /// <summary>
    /// Sipariş Id (FK)
    /// </summary>
    public long OrderId { get; set; }
    
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Ürün adı (sipariş anındaki)
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Miktar
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// İskonto tutarı
    /// </summary>
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// Satır toplamı
    /// </summary>
    public decimal LineTotal { get; set; }
    
    /// <summary>
    /// Vergi oranı
    /// </summary>
    public decimal TaxRate { get; set; }
    
    /// <summary>
    /// Vergi tutarı
    /// </summary>
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Vergiye tabi tutar
    /// </summary>
    public decimal TaxableAmount { get; set; }
    
    /// <summary>
    /// Net toplam
    /// </summary>
    public decimal NetTotal { get; set; }
    
    /// <summary>
    /// Depo Id (FK)
    /// </summary>
    public int WarehouseId { get; set; }
    
    /// <summary>
    /// Satır notu
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Oluşturan kullanıcı adı
    /// </summary>
    public string? CreatedByUsername { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// İlişkili depo
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
}
