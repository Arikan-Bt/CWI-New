namespace CWI.Application.DTOs.Purchasing;

public class CreateVendorInvoiceResponse
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? FilePath { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class VendorInvoiceDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoicedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Description { get; set; }
    public string? FilePath { get; set; }
    public decimal Balance { get; set; }
}
