namespace CWI.Application.DTOs.Reports;

/// <summary>
/// Cancel edilmiş siparişler için debit note seçim öğesi.
/// </summary>
public class CancelledInvoiceOptionDto
{
    public long OrderId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime CanceledDate { get; set; }
    public decimal PaidAmount { get; set; }
}

/// <summary>
/// Cancel edilmiş siparişlerin liste yanıtı.
/// </summary>
public class CancelledInvoicesResponse
{
    public List<CancelledInvoiceOptionDto> Data { get; set; } = new();
}

/// <summary>
/// Debit note oluşturma ve excel indirme isteği.
/// </summary>
public class CreateDebitNoteExportRequest
{
    public string CustomerCode { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DebitNoteDate { get; set; }
    public string? Notes { get; set; }
}
