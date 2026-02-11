using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;

namespace CWI.Domain.Entities.Purchasing;

/// <summary>
/// Mal alım fişi entity'si (eski: cdPurchase)
/// </summary>
public class GoodsReceipt : AuditableLongEntity
{
    /// <summary>
    /// Fiş numarası
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Fiş tarihi
    /// </summary>
    public DateTime ReceiptDate { get; set; }
    
    /// <summary>
    /// Satın alma siparişi Id (FK) - opsiyonel
    /// </summary>
    public long? PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Tedarikçi Id (FK)
    /// </summary>
    public int? SupplierId { get; set; }
    
    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Notlar
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili satın alma siparişi
    /// </summary>
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    
    /// <summary>
    /// İlişkili tedarikçi
    /// </summary>
    public virtual Customer? Supplier { get; set; }
    
    /// <summary>
    /// Fiş kalemleri
    /// </summary>
    public virtual ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
}
