using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;

namespace CWI.Domain.Entities.Purchasing;

/// <summary>
/// Satın alma siparişi entity'si (eski: cdCustomerOrderHeader)
/// </summary>
public class PurchaseOrder : AuditableLongEntity
{
    /// <summary>
    /// Sipariş numarası
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Seri numarası
    /// </summary>
    public string? SerialNumber { get; set; }
    
    /// <summary>
    /// Belge numarası
    /// </summary>
    public int DocumentNumber { get; set; }
    
    /// <summary>
    /// Sipariş tarihi
    /// </summary>
    public DateTime OrderedAt { get; set; }

    /// <summary>
    /// Teslim tarihi
    /// </summary>
    public DateTime? DeliveryDate { get; set; }
    
    /// <summary>
    /// Toplam miktar
    /// </summary>
    public int TotalQuantity { get; set; }
    
    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Tedarikçi adı
    /// </summary>
    public string? SupplierName { get; set; }
    
    /// <summary>
    /// Harici referans numarası
    /// </summary>
    public string? ExternalReference { get; set; }
    
    /// <summary>
    /// Teslim alındı mı?
    /// </summary>
    public bool IsReceived { get; set; }
    
    /// <summary>
    /// Tedarikçi Id (FK) - opsiyonel
    /// </summary>
    public int? SupplierId { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili tedarikçi (Customer olarak kayıtlı)
    /// </summary>
    public virtual Customer? Supplier { get; set; }
    
    /// <summary>
    /// Sipariş kalemleri
    /// </summary>
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
