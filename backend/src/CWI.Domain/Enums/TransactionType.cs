namespace CWI.Domain.Enums;

/// <summary>
/// Cari hareket tipi enum'ı
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Fatura
    /// </summary>
    Invoice = 1,
    
    /// <summary>
    /// Ödeme
    /// </summary>
    Payment = 2,
    
    /// <summary>
    /// Alacak dekontu
    /// </summary>
    CreditNote = 3,
    
    /// <summary>
    /// Borç dekontu
    /// </summary>
    DebitNote = 4,
    
    /// <summary>
    /// Açılış bakiyesi
    /// </summary>
    OpeningBalance = 5
}
