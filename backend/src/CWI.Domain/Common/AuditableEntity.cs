namespace CWI.Domain.Common;

/// <summary>
/// Oluşturma ve güncelleme tarihi takibi için interface
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Kullanıcı bilgisi ile birlikte audit takibi için interface
/// </summary>
public interface IUserAuditableEntity : IAuditableEntity
{
    /// <summary>
    /// Oluşturan kullanıcı adı
    /// </summary>
    string? CreatedByUsername { get; set; }
}

/// <summary>
/// Audit bilgisi içeren temel entity sınıfı (int Id)
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Audit bilgisi içeren temel entity sınıfı (long Id)
/// </summary>
public abstract class AuditableLongEntity : BaseLongEntity, IAuditableEntity
{
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
