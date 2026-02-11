using CWI.Domain.Common;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Rol entity'si (eski: cdUserGroup)
/// </summary>
public class Role : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Rol kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Rol adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Rol açıklaması
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Yönetici rolü mü?
    /// </summary>
    public bool IsAdmin { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Bu role sahip kullanıcılar
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>
    /// Bu role atanan yetkiler
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
