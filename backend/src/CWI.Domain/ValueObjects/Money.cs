namespace CWI.Domain.ValueObjects;

/// <summary>
/// Para birimi ile tutar bilgisi için Value Object
/// </summary>
public sealed record Money
{
    /// <summary>
    /// Tutar değeri
    /// </summary>
    public decimal Amount { get; init; }
    
    /// <summary>
    /// Para birimi kodu (örn: TRY, USD, EUR)
    /// </summary>
    public string CurrencyCode { get; init; } = "TRY";
    
    /// <summary>
    /// Sıfır tutar oluşturur
    /// </summary>
    public static Money Zero(string currencyCode = "TRY") 
        => new() { Amount = 0, CurrencyCode = currencyCode };
    
    /// <summary>
    /// İki tutarı toplar (aynı para birimi kontrolü yapar)
    /// </summary>
    public Money Add(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new InvalidOperationException("Farklı para birimleri toplanamaz.");
        
        return this with { Amount = Amount + other.Amount };
    }
    
    /// <summary>
    /// Tutarı çıkarır (aynı para birimi kontrolü yapar)
    /// </summary>
    public Money Subtract(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new InvalidOperationException("Farklı para birimleri çıkarılamaz.");
        
        return this with { Amount = Amount - other.Amount };
    }
    
    public override string ToString() => $"{Amount:N2} {CurrencyCode}";
}
