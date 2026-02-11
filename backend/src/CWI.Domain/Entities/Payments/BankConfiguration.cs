using CWI.Domain.Common;

namespace CWI.Domain.Entities.Payments;

/// <summary>
/// Banka konfigürasyonu (POS ayarları) entity'si (eski: cdBankSettings)
/// </summary>
public class BankConfiguration : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Banka kodu
    /// </summary>
    public string BankCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Banka adı
    /// </summary>
    public string BankName { get; set; } = string.Empty;
    
    /// <summary>
    /// Merchant ID
    /// </summary>
    public string? MerchantId { get; set; }
    
    /// <summary>
    /// Terminal ID
    /// </summary>
    public string? TerminalId { get; set; }
    
    /// <summary>
    /// Kullanıcı adı
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Şifre (şifrelenmiş)
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// API URL
    /// </summary>
    public string? ApiUrl { get; set; }
    
    /// <summary>
    /// 3D Secure aktif mi?
    /// </summary>
    public bool Is3DSecureEnabled { get; set; }
    
    /// <summary>
    /// Sıralama değeri
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// Banka BIN kodları
    /// </summary>
    public virtual ICollection<BankBinCode> BinCodes { get; set; } = new List<BankBinCode>();
}
