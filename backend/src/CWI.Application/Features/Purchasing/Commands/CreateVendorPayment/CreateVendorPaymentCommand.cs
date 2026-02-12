using CWI.Application.DTOs.Purchasing;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CWI.Application.Features.Purchasing.Commands.CreateVendorPayment;

public class CreateVendorPaymentCommand : IRequest<CreateVendorPaymentResponse>
{
    public string VendorCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public IFormFile? PaymentFile { get; set; }
}
