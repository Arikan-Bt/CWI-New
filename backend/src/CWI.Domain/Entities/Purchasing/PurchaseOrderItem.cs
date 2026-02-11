using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Purchasing;

/// <summary>
/// Satın alma siparişi kalemi entity'si (eski: cdCustomerOrderLine)
/// </summary>
public class PurchaseOrderItem : BaseLongEntity
{
    /// <summary>
    /// Satın alma siparişi Id (FK)
    /// </summary>
    public long PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Ürün kodu
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün adı
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Miktar
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Teslim alınan miktar
    /// </summary>
    public int ReceivedQuantity { get; set; }
    
    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Satır toplamı
    /// </summary>
    public decimal LineTotal { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili satın alma siparişi
    /// </summary>
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    
    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}
