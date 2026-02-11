using CWI.Domain.Common;

namespace CWI.Domain.Entities.Identity;

/// <summary>
/// Satış hedefi entity'si (eski: cdSalesPersonTargets)
/// </summary>
public class SalesTarget : AuditableEntity
{
    /// <summary>
    /// Kullanıcı Id (FK)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Yıl
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Ay
    /// </summary>
    public int Month { get; set; }
    
    /// <summary>
    /// Hedef tutar
    /// </summary>
    public decimal TargetAmount { get; set; }
    
    /// <summary>
    /// Gerçekleşen tutar
    /// </summary>
    public decimal ActualAmount { get; set; }
    
    /// <summary>
    /// Hedef adet
    /// </summary>
    public int? TargetQuantity { get; set; }
    
    /// <summary>
    /// Gerçekleşen adet
    /// </summary>
    public int? ActualQuantity { get; set; }
    
    /// <summary>
    /// Başarı oranı (computed)
    /// </summary>
    public decimal AchievementRate => TargetAmount > 0 ? (ActualAmount / TargetAmount) * 100 : 0;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili kullanıcı
    /// </summary>
    public virtual User User { get; set; } = null!;
}
