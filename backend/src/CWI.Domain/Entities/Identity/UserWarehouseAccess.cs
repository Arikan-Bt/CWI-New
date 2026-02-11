using CWI.Domain.Common;
using CWI.Domain.Entities.Inventory;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Kullanıcı-Depo erişim yetkisi entity'si (eski: cdSalesPersonWareHouses)
/// </summary>
public class UserWarehouseAccess : BaseEntity
{
    /// <summary>
    /// Kullanıcı Id (FK)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Depo Id (FK)
    /// </summary>
    public int WarehouseId { get; set; }
    
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
    /// İlişkili depo
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
}
