using CWI.Domain.Common;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Payments;

namespace CWI.Domain.Entities.Customers;

/// <summary>
/// Müşteri entity'si (eski: cdCurrAcc)
/// </summary>
public class Customer : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Müşteri kodu (Business Key)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Müşteri adı/unvanı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Vergi dairesi adı
    /// </summary>
    public string? TaxOfficeName { get; set; }
    
    /// <summary>
    /// Vergi numarası
    /// </summary>
    public string? TaxNumber { get; set; }
    
    /// <summary>
    /// Bölge kodu
    /// </summary>
    public string? RegionCode { get; set; }
    
    /// <summary>
    /// Bölge adı
    /// </summary>
    public string? RegionName { get; set; }
    
    /// <summary>
    /// Adres satırı 1
    /// </summary>
    public string? AddressLine1 { get; set; }
    
    /// <summary>
    /// Adres satırı 2
    /// </summary>
    public string? AddressLine2 { get; set; }
    
    /// <summary>
    /// Mahalle/Semt
    /// </summary>
    public string? District { get; set; }
    
    /// <summary>
    /// İlçe
    /// </summary>
    public string? Town { get; set; }
    
    /// <summary>
    /// Şehir
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Ülke
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Birincil telefon numarası
    /// </summary>
    public string? PrimaryPhone { get; set; }
    
    /// <summary>
    /// İkincil telefon numarası
    /// </summary>
    public string? SecondaryPhone { get; set; }
    
    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Müşteri aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Bu bir tedarikçi (vendor) mi?
    /// </summary>
    public bool IsVendor { get; set; } = false;
    
    // Navigation Properties
    
    /// <summary>
    /// Müşteriye ait siparişler
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    
    /// <summary>
    /// Müşteri cari hareketleri
    /// </summary>
    public virtual ICollection<CustomerTransaction> Transactions { get; set; } = new List<CustomerTransaction>();
    
    /// <summary>
    /// Müşteri iletişim bilgileri
    /// </summary>
    public virtual ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
    
    /// <summary>
    /// Müşteri fiyat kuralları
    /// </summary>
    public virtual ICollection<CustomerPricing> PricingRules { get; set; } = new List<CustomerPricing>();
    
    /// <summary>
    /// Müşteri iskonto tanımları
    /// </summary>
    public virtual ICollection<CustomerDiscount> Discounts { get; set; } = new List<CustomerDiscount>();
    
    /// <summary>
    /// Müşteriye ait ödemeler
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
