using CWI.Domain.Common;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Rol ve yetki eşleşmesini tutan entity.
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// Rol ID
    /// </summary>
    public int RoleId { get; set; }
    
    /// <summary>
    /// Yetki anahtarı (Permissions sabitlerinden gelir)
    /// </summary>
    public string PermissionKey { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual Role Role { get; set; } = null!;
}
