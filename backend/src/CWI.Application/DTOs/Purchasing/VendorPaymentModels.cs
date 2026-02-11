namespace CWI.Application.DTOs.Purchasing;

public class CreateVendorPaymentResponse
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class VendorPaymentDto
{
    public long Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTime PaidAt { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public string? FilePath { get; set; }
}
