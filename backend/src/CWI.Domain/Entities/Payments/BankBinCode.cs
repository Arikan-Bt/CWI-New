using CWI.Domain.Common;

namespace CWI.Domain.Entities.Payments;

/// <summary>
/// Banka BIN kodu entity'si (eski: cdBankBinCodes)
/// </summary>
public class BankBinCode : BaseEntity
{
    /// <summary>
    /// Banka konfigürasyonu Id (FK)
    /// </summary>
    public int BankConfigurationId { get; set; }
    
    /// <summary>
    /// BIN kodu (kartın ilk 6 hanesi)
    /// </summary>
    public string BinCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Kart tipi (Credit, Debit, Prepaid)
    /// </summary>
    public string? CardType { get; set; }
    
    /// <summary>
    /// Kart markası (Visa, Mastercard, Troy)
    /// </summary>
    public string? CardBrand { get; set; }
    
    /// <summary>
    /// Ticari kart mı?
    /// </summary>
    public bool IsCommercial { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// İlişkili banka konfigürasyonu
    /// </summary>
    public virtual BankConfiguration BankConfiguration { get; set; } = null!;
}
