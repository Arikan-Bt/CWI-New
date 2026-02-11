using CWI.Domain.Common;

namespace CWI.Domain.Entities.Lookups;

/// <summary>
/// Sevkiyat koşulu entity'si (eski: cdShipmentTerms)
/// </summary>
public class ShipmentTerm : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Koşul kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Koşul adı
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Açıklama
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
