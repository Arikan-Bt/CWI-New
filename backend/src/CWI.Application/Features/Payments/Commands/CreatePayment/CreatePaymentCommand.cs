using CWI.Application.DTOs.Payments;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CWI.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommand : IRequest<CreatePaymentResponse>
{
    public string CustomerCode { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public string? PaymentMethodCode { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal Amount { get; set; }
    public string? ReceiptNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public IFormFile? ReceiptFile { get; set; }
}
