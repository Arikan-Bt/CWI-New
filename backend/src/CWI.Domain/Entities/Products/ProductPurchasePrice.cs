using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Payments;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün alış fiyatı (Vendor Price List)
/// </summary>
public class ProductPurchasePrice : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Tedarikçi (Müşteri) Id (FK)
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Para birimi Id (FK)
    /// </summary>
    public int CurrencyId { get; set; }

    /// <summary>
    /// Geçerlilik başlangıç tarihi
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Geçerlilik bitiş tarihi (null ise süresiz)
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// İlişkili ürün
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// İlişkili tedarikçi (Müşteri tablosundan)
    /// </summary>
    public virtual Customer Vendor { get; set; } = null!;

    /// <summary>
    /// İlişkili para birimi
    /// </summary>
    public virtual Currency Currency { get; set; } = null!;
}
