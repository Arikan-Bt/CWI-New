using CWI.Application.DTOs.Purchasing;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CWI.Application.Features.Purchasing.Commands.CreateVendorInvoice;

public class CreateVendorInvoiceCommand : IRequest<CreateVendorInvoiceResponse>
{
    public string VendorCode { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal Amount { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile? InvoiceFile { get; set; }
}
