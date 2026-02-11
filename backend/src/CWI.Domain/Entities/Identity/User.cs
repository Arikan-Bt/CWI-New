using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Enums;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Kullanıcı entity'si (eski: cdUser)
/// ASP.NET Identity ile birlikte kullanılmak üzere tasarlandı
/// </summary>
public class User : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Kullanıcı adı (benzersiz)
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Şifre hash'i
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Personel kodu
    /// </summary>
    public string? EmployeeCode { get; set; }
    
    /// <summary>
    /// Ad
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Soyad
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Ofis kodu
    /// </summary>
    public string? OfficeCode { get; set; }
    
    /// <summary>
    /// Satış temsilcisi kodu
    /// </summary>
    public string? SalesRepCode { get; set; }
    
    /// <summary>
    /// Bağlı müşteri Id (FK) - B2B kullanıcıları için
    /// </summary>
    public int? LinkedCustomerId { get; set; }
    
    /// <summary>
    /// Yönetici mi?
    /// </summary>
    public bool IsAdministrator { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Rol Id (FK)
    /// </summary>
    public int RoleId { get; set; }
    
    /// <summary>
    /// Proje tipi (CWI, AWC vb.)
    /// </summary>
    public ProjectType ProjectType { get; set; }
    
    /// <summary>
    /// Yenileme token'ı
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Yenileme token'ı son geçerlilik tarihi
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    /// <summary>
    /// Kısıtlı markalar (Comma separated IDs)
    /// </summary>
    public string? RestrictedBrands { get; set; }

    /// <summary>
    /// İzin verilen ürünler (Comma separated SKUs)
    /// </summary>
    public string? AllowedProducts { get; set; }

    /// <summary>
    /// Bloklanmış ürünler (Comma separated SKUs)
    /// </summary>
    public string? BlockedProducts { get; set; }
    
    /// <summary>
    /// Tam ad (computed)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    // Navigation Properties
    
    /// <summary>
    /// Bağlı müşteri
    /// </summary>
    public virtual Customer? LinkedCustomer { get; set; }
    
    /// <summary>
    /// Kullanıcı rolü
    /// </summary>
    public virtual Role Role { get; set; } = null!;
    
    /// <summary>
    /// Marka erişimleri
    /// </summary>
    public virtual ICollection<UserBrandAccess> BrandAccess { get; set; } = new List<UserBrandAccess>();
    
    /// <summary>
    /// Bölge erişimleri
    /// </summary>
    public virtual ICollection<UserRegionAccess> RegionAccess { get; set; } = new List<UserRegionAccess>();
    
    /// <summary>
    /// Depo erişimleri
    /// </summary>
    public virtual ICollection<UserWarehouseAccess> WarehouseAccess { get; set; } = new List<UserWarehouseAccess>();
    
    /// <summary>
    /// Satış hedefleri
    /// </summary>
    public virtual ICollection<SalesTarget> SalesTargets { get; set; } = new List<SalesTarget>();
    
    /// <summary>
    /// Giriş geçmişi
    /// </summary>
    public virtual ICollection<UserLoginHistory> LoginHistory { get; set; } = new List<UserLoginHistory>();
}
