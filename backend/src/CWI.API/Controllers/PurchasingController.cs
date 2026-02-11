using CWI.Application.Common.Models;
using CWI.Application.DTOs.Purchasing;
using CWI.Application.Features.Purchasing.Commands.CreateVendorInvoice;
using CWI.Application.Features.Purchasing.Commands.CreateVendorPayment;
using CWI.Application.Features.Purchasing.Queries.GetVendorBalanceReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CWI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CWI.API.Controllers;

[Route("[controller]")]
[ApiController]
public class PurchasingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CWIDbContext _context;

    public PurchasingController(IMediator mediator, CWIDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet("debug-count")]
    public async Task<IActionResult> GetDebugCount()
    {
        var count = await _context.VendorInvoices.CountAsync();
        var invoices = await _context.VendorInvoices
            .Include(i => i.Vendor)
            .Include(i => i.Currency)
            .Select(i => new { i.InvoiceNumber, i.InvoicedAt, Vendor = i.Vendor.Name, Currency = i.Currency.Code })
            .ToListAsync();
        
        return Ok(Result<object>.Succeed(new { Count = count, Invoices = invoices }));
    }

    /// <summary>
    /// Yeni bir satınalma faturası oluşturur.
    /// </summary>
    /// <param name="command">Fatura oluşturma komutu</param>
    /// <returns>Oluşturulan faturanın sonucu</returns>
    [HttpPost("invoices")]
    public async Task<ActionResult<Result<CreateVendorInvoiceResponse>>> CreateInvoice([FromForm] CreateVendorInvoiceCommand command)
    {
        var response = await _mediator.Send(command);
        if (response.Success)
        {
            return Ok(Result<CreateVendorInvoiceResponse>.Succeed(response));
        }
        return BadRequest(Result<CreateVendorInvoiceResponse>.Failure(response.Message));
    }

    /// <summary>
    /// Tedarikçiye yapılan ödemeyi kaydeder.
    /// </summary>
    /// <param name="command">Ödeme kayıt komutu</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost("payments")]
    public async Task<ActionResult<Result<CreateVendorPaymentResponse>>> CreatePayment([FromForm] CreateVendorPaymentCommand command)
    {
        var response = await _mediator.Send(command);
        if (response.Success)
        {
            return Ok(Result<CreateVendorPaymentResponse>.Succeed(response));
        }
        return BadRequest(Result<CreateVendorPaymentResponse>.Failure(response.Message));
    }

    /// <summary>
    /// Tedarikçi bakiye raporunu getirir.
    /// </summary>
    /// <param name="request">Filtreleme kriterleri</param>
    /// <returns>Bakiye raporu listesi</returns>
    [HttpPost("balance-report")]
    public async Task<ActionResult<Result<VendorBalanceReportResponse>>> GetVendorBalanceReport([FromBody] VendorBalanceReportRequest request)
    {
        var result = await _mediator.Send(new GetVendorBalanceReportQuery { Request = request });
        return Ok(Result<VendorBalanceReportResponse>.Succeed(result));
    }
}

