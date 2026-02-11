namespace CWI.Domain.ValueObjects;

/// <summary>
/// Adres bilgisi için Value Object
/// </summary>
public sealed record Address
{
    /// <summary>
    /// Adres satırı 1
    /// </summary>
    public string? Line1 { get; init; }
    
    /// <summary>
    /// Adres satırı 2
    /// </summary>
    public string? Line2 { get; init; }
    
    /// <summary>
    /// Mahalle/Semt
    /// </summary>
    public string? District { get; init; }
    
    /// <summary>
    /// İlçe
    /// </summary>
    public string? Town { get; init; }
    
    /// <summary>
    /// Şehir
    /// </summary>
    public string? City { get; init; }
    
    /// <summary>
    /// Ülke
    /// </summary>
    public string? Country { get; init; }
    
    /// <summary>
    /// Posta kodu
    /// </summary>
    public string? PostalCode { get; init; }
    
    /// <summary>
    /// Tam adres metnini döndürür
    /// </summary>
    public string FullAddress => string.Join(", ", 
        new[] { Line1, Line2, District, Town, City, Country, PostalCode }
        .Where(x => !string.IsNullOrWhiteSpace(x)));
}
