namespace CWI.Application.DTOs.Inventory;

/// <summary>
/// Depo oluşturmak için DTO
/// </summary>
public class CreateWarehouseDto
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
    /// Varsayılan depo mu?
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// Depo güncellemek için DTO
/// </summary>
public class UpdateWarehouseDto
{
    /// <summary>
    /// Depo ID
    /// </summary>
    public int Id { get; set; }
    
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
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Varsayılan depo mu?
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// Depo detay bilgilerini döndürmek için DTO (mevcut WarehouseDto genişletilmiş hali)
/// </summary>
public class WarehouseDetailDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
