using CWI.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace CWI.Application.DTOs.Payments;

public class CreatePaymentRequest
{
    public int CustomerId { get; set; }
    public string? PaymentMethodCode { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal Amount { get; set; }
    public string? ReceiptNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public IFormFile? ReceiptFile { get; set; }
}

public class CreatePaymentResponse
{
    public long PaymentId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PaymentFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PaymentStatus? Status { get; set; }
    public string? CurrAccCode { get; set; }
    public string? ProjectType { get; set; }
}

public class PaymentListDto
{
    public long Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReceiptFilePath { get; set; }
    public string? Notes { get; set; }
}

public class PaymentDetailDto : PaymentListDto
{
    // Detay için ek alanlar gerekirse buraya
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UploadReceiptDto
{
    public IFormFile File { get; set; } = null!;
}
