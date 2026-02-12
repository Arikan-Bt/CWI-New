using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Payments;

namespace CWI.Domain.Entities.Products;

/// <summary>
/// Ürün satış fiyatı (Sales Price List)
/// </summary>
public class ProductSalesPrice : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Ürün Id (FK)
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Müşteri Id (FK)
    /// </summary>
    public int CustomerId { get; set; }

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
    /// İlişkili müşteri
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// İlişkili para birimi
    /// </summary>
    public virtual Currency Currency { get; set; } = null!;
}
