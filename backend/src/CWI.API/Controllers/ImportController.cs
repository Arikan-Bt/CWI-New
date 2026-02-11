using CWI.Application.Common.Models;
using CWI.Application.Features.Import.Commands.ImportOrder;
using CWI.Application.Features.Import.Queries.DownloadTemplate;
using CWI.Application.Features.Import.Queries.ValidateOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ImportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("validate-order")]
    public async Task<IActionResult> ValidateOrder([FromBody] ValidateOrderQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(Result<ValidateOrderResponse>.Succeed(result));
    }

    [HttpPost("orders")]
    public async Task<IActionResult> ImportOrders([FromBody] ImportOrderCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.SuccessCount == 0 && result.ErrorCount > 0)
        {
            var errors = result.Errors
                .Select(e => e.Row > 0 ? $"Row {e.Row}: {e.Message}" : e.Message)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            return Ok(Result<ImportOrderResponse>.Failure(
                errors.Count > 0 ? errors : new[] { "Excel import failed." }));
        }

        return Ok(Result<ImportOrderResponse>.Succeed(result));
    }

    [HttpGet("orders/template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var fileContent = await _mediator.Send(new DownloadOrderTemplateQuery());
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrderTemplate.xlsx");
    }
}
