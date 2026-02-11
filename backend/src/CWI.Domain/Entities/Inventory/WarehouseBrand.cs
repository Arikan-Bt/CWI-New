using CWI.Domain.Common;
using CWI.Domain.Entities.Products;

namespace CWI.Domain.Entities.Inventory;

/// <summary>
/// Depo-Marka ilişkisi entity'si (eski: trBrands)
/// </summary>
public class WarehouseBrand : BaseEntity
{
    /// <summary>
    /// Depo Id (FK)
    /// </summary>
    public int WarehouseId { get; set; }
    
    /// <summary>
    /// Marka Id (FK)
    /// </summary>
    public int BrandId { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili depo
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
    
    /// <summary>
    /// İlişkili marka
    /// </summary>
    public virtual Brand Brand { get; set; } = null!;
}
