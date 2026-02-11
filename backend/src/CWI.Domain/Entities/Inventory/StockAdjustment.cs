using CWI.Domain.Common;

namespace CWI.Domain.Entities.Inventory;

/// <summary>
/// Stok düzenleme ana kaydı
/// </summary>
public class StockAdjustment : BaseLongEntity
{
    /// <summary>
    /// Düzenleme tarihi
    /// </summary>
    public DateTime AdjustmentDate { get; set; }
    
    /// <summary>
    /// Açıklama
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Yüklenen dosya yolu
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// Kayıt tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
