using CWI.Domain.Common;

namespace CWI.Domain.Entities.Orders;

/// <summary>
/// Sipariş paketi (koli başlık) entity'si (eski: trShopCartoon)
/// </summary>
public class OrderPackage : AuditableLongEntity
{
    /// <summary>
    /// Sipariş Id (FK)
    /// </summary>
    public long OrderId { get; set; }
    
    /// <summary>
    /// Paket numarası
    /// </summary>
    public string PackageNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Ağırlık (kg) - Gross Weight olarak kullanılabilir veya legacy
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Net Ağırlık (kg)
    /// </summary>
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// Brüt Ağırlık (kg)
    /// </summary>
    public decimal? GrossWeight { get; set; }
    
    /// <summary>
    /// Uzunluk (cm)
    /// </summary>
    public decimal? Length { get; set; }
    
    /// <summary>
    /// Genişlik (cm)
    /// </summary>
    public decimal? Width { get; set; }
    
    /// <summary>
    /// Yükseklik (cm)
    /// </summary>
    public decimal? Height { get; set; }
    
    /// <summary>
    /// Takip numarası
    /// </summary>
    public string? TrackingNumber { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili sipariş
    /// </summary>
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// Paket içerikleri
    /// </summary>
    public virtual ICollection<OrderPackageItem> Items { get; set; } = new List<OrderPackageItem>();
}
