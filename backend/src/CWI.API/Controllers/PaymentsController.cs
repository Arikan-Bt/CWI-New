using CWI.Application.Common.Models;
using CWI.Application.DTOs.Payments;
using CWI.Application.Features.Payments.Commands.CreatePayment;
using CWI.Application.Features.Payments.Commands.ApprovePayment;
using CWI.Application.Features.Payments.Commands.RejectPayment;
using CWI.Application.Features.Payments.Commands.UploadReceipt;
using CWI.Application.Features.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Yeni bir ödeme bildirimi oluşturur.
    /// </summary>
    /// <param name="command">Ödeme bilgileri ve makbuz dosyası</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<CreatePaymentResponse>>> CreatePayment([FromForm] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(Result<CreatePaymentResponse>.Succeed(result));
        }
        return BadRequest(Result<CreatePaymentResponse>.Failure(result.Message));
    }

    [HttpGet]
    public async Task<ActionResult<Result<PaymentListResponse>>> GetPayments([FromQuery] PaymentFilterDto filter)
    {
        var result = await _mediator.Send(new GetPaymentsQuery { Filter = filter });
        return Ok(Result<PaymentListResponse>.Succeed(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<PaymentDetailDto>>> GetPayment(long id)
    {
        var result = await _mediator.Send(new GetPaymentDetailQuery { Id = id });
        if (result == null) return NotFound(Result<PaymentDetailDto>.Failure("Payment not found."));
        return Ok(Result<PaymentDetailDto>.Succeed(result));
    }

    [HttpPost("{id}/receipt")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<CreatePaymentResponse>>> UploadReceipt(long id, [FromForm] UploadReceiptDto dto)
    {
        var command = new UploadPaymentReceiptCommand { PaymentId = id, File = dto.File };
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(Result<CreatePaymentResponse>.Succeed(result));
        return BadRequest(Result<CreatePaymentResponse>.Failure(result.Message));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<Result<bool>>> ApprovePayment(long id)
    {
        var result = await _mediator.Send(new ApprovePaymentCommand { PaymentId = id });
        return Ok(Result<bool>.Succeed(result));
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<Result<bool>>> RejectPayment(long id, [FromBody] RejectPaymentCommand command)
    {
        if (id != command.PaymentId && command.PaymentId == 0) command.PaymentId = id;
        var result = await _mediator.Send(command);
        return Ok(Result<bool>.Succeed(result));
    }
}
