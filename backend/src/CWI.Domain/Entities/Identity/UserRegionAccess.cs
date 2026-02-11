using CWI.Domain.Common;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Kullanıcı-Bölge erişim yetkisi entity'si (eski: cdSalesPersonRegion)
/// </summary>
public class UserRegionAccess : BaseEntity
{
    /// <summary>
    /// Kullanıcı Id (FK)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Bölge kodu
    /// </summary>
    public string RegionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Bölge adı
    /// </summary>
    public string? RegionName { get; set; }
    
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
}
