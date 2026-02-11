using CWI.Domain.Enums;

namespace CWI.Application.DTOs.Brands;

/// <summary>
/// Marka detay DTO
/// </summary>
public record BrandDetailDto
{
    /// <summary>Marka Id</summary>
    public int Id { get; init; }
    
    /// <summary>Marka kodu</summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>Marka adı</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Logo URL</summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>Sıralama değeri</summary>
    public int SortOrder { get; init; }
    
    /// <summary>Aktif mi?</summary>
    public bool IsActive { get; init; }
    
    /// <summary>Proje tipi</summary>
    public ProjectType ProjectType { get; init; }
    
    /// <summary>Proje tipi adı (görüntüleme için)</summary>
    public string ProjectTypeName => ProjectType.ToString();
}

/// <summary>
/// Marka oluşturma DTO
/// </summary>
public record CreateBrandDto
{
    /// <summary>Marka kodu</summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>Marka adı</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Logo URL</summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>Sıralama değeri</summary>
    public int SortOrder { get; init; }
    
    /// <summary>Aktif mi?</summary>
    public bool IsActive { get; init; } = true;
    
    /// <summary>Proje tipi</summary>
    public ProjectType ProjectType { get; init; }
}

/// <summary>
/// Marka güncelleme DTO
/// </summary>
public record UpdateBrandDto
{
    /// <summary>Marka Id</summary>
    public int Id { get; init; }
    
    /// <summary>Marka kodu</summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>Marka adı</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Logo URL</summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>Sıralama değeri</summary>
    public int SortOrder { get; init; }
    
    /// <summary>Aktif mi?</summary>
    public bool IsActive { get; init; }
    
    /// <summary>Proje tipi</summary>
    public ProjectType ProjectType { get; init; }
}

/// <summary>
/// Marka listesi sayfalama yanıtı
/// </summary>
public record BrandListResponse
{
    /// <summary>Marka listesi</summary>
    public List<BrandDetailDto> Data { get; init; } = new();
    
    /// <summary>Toplam kayıt sayısı</summary>
    public int TotalCount { get; init; }
}
