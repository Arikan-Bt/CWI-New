namespace CWI.Domain.ValueObjects;

/// <summary>
/// Tarih aralığı için Value Object
/// </summary>
public sealed record DateRange
{
    /// <summary>
    /// Başlangıç tarihi
    /// </summary>
    public DateTime Start { get; init; }
    
    /// <summary>
    /// Bitiş tarihi (opsiyonel - null ise süresiz)
    /// </summary>
    public DateTime? End { get; init; }
    
    /// <summary>
    /// Tarih aralığının aktif olup olmadığını kontrol eder
    /// </summary>
    public bool IsActive => Start <= DateTime.UtcNow && (!End.HasValue || End.Value >= DateTime.UtcNow);
    
    /// <summary>
    /// Belirtilen tarihin aralık içinde olup olmadığını kontrol eder
    /// </summary>
    public bool Contains(DateTime date) => date >= Start && (!End.HasValue || date <= End.Value);
    
    /// <summary>
    /// Aralığın gün cinsinden süresini hesaplar
    /// </summary>
    public int? DurationInDays => End.HasValue ? (int?)(End.Value - Start).TotalDays : null;
}
