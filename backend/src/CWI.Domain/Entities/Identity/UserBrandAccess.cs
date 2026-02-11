using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Kullanıcı-Marka erişim yetkisi entity'si (eski: cdUserBrand + cdUserBrands)
/// </summary>
public class UserBrandAccess : BaseEntity
{
    /// <summary>
    /// Kullanıcı Id (FK)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Marka Id (FK)
    /// </summary>
    public int BrandId { get; set; }
    
    /// <summary>
    /// Yetki verilme tarihi
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili kullanıcı
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// İlişkili marka
    /// </summary>
    public virtual Brand Brand { get; set; } = null!;
}
