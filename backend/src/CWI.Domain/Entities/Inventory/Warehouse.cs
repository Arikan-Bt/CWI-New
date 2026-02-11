using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Inventory;

/// <summary>
/// Depo entity'si (eski: cdWareHouse)
/// </summary>
public class Warehouse : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Depo kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Depo adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Depo adresi
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Varsayılan depo mu?
    /// </summary>
    public bool IsDefault { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Depodaki stok kalemleri
    /// </summary>
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    
    /// <summary>
    /// Depo-marka ilişkileri
    /// </summary>
    public virtual ICollection<WarehouseBrand> WarehouseBrands { get; set; } = new List<WarehouseBrand>();
}
