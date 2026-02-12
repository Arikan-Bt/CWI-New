using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Enums;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş entity'si (eski: trShopCartHeader + PreOrderShopCartHeader)
/// </summary>
public class Order : AuditableLongEntity, IUserAuditableEntity
{
    /// <summary>
    /// Sipariş numarası
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Sipariş tarihi
    /// </summary>
    public DateTime OrderedAt { get; set; }
    
    /// <summary>
    /// Müşteri Id (FK)
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Satış temsilcisi
    /// </summary>
    public string? SalesRepresentative { get; set; }
    
    /// <summary>
    /// Toplam adet
    /// </summary>
    public decimal TotalQuantity { get; set; }
    
    /// <summary>
    /// Ara toplam
    /// </summary>
    public decimal SubTotal { get; set; }
    
    /// <summary>
    /// Toplam iskonto
    /// </summary>
    public decimal TotalDiscount { get; set; }
    
    /// <summary>
    /// Vergiye tabi tutar
    /// </summary>
    public decimal TaxableAmount { get; set; }
    
    /// <summary>
    /// Genel toplam
    /// </summary>
    public decimal GrandTotal { get; set; }
    
    /// <summary>
    /// Sipariş durumu
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    /// <summary>
    /// Tamamlandı mı?
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Onaylandı mı?
    /// </summary>
    public bool IsApproved { get; set; }
    
    /// <summary>
    /// İptal edildi mi?
    /// </summary>
    public bool IsCanceled { get; set; }
    
    /// <summary>
    /// İptal nedeni
    /// </summary>
    public string? CancellationReason { get; set; }
    
    /// <summary>
    /// Sipariş notları
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Yüklenen sipariş dosyası yolu
    /// </summary>
    public string? OrderFilePath { get; set; }
    
    /// <summary>
    /// Ön sipariş mi?
    /// </summary>
    public bool IsPreOrder { get; set; }
    
    /// <summary>
    /// Oluşturan kullanıcı grup kodu
    /// </summary>
    public string? CreatedByGroupCode { get; set; }
    
    /// <summary>
    /// Oluşturan kullanıcı adı
    /// </summary>
    public string? CreatedByUsername { get; set; }
    
    /// <summary>
    /// Sevk tarihi
    /// </summary>
    public DateTime? ShippedAt { get; set; }
    
    /// <summary>
    /// Para birimi Id (FK)
    /// </summary>
    public int CurrencyId { get; set; }

    /// <summary>
    /// Sezon bilgisi
    /// </summary>
    public string? Season { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili müşteri
    /// </summary>
    public virtual Customer? Customer { get; set; }
    
    /// <summary>
    /// İlişkili para birimi
    /// </summary>
    public virtual Currency Currency { get; set; } = null!;
    
    /// <summary>
    /// Sipariş kalemleri
    /// </summary>
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    
    /// <summary>
    /// Teslimat bilgisi
    /// </summary>
    public virtual OrderShippingInfo? ShippingInfo { get; set; }
    
    /// <summary>
    /// Teslimat talebi
    /// </summary>
    public virtual OrderDeliveryRequest? DeliveryRequest { get; set; }
    
    /// <summary>
    /// Sipariş paketleri (koliler)
    /// </summary>
    public virtual ICollection<OrderPackage> Packages { get; set; } = new List<OrderPackage>();
    
    /// <summary>
    /// Vergi detayları
    /// </summary>
    public virtual ICollection<OrderTaxDetail> TaxDetails { get; set; } = new List<OrderTaxDetail>();
    
    /// <summary>
    /// Ödemeler
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    /// <summary>
    /// ERP senkron bilgisi
    /// </summary>
    public virtual OrderErpSync? ErpSync { get; set; }
}
