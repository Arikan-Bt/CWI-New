using CWI.Application.DTOs.Customers;
using CWI.Application.Features.Customers.Commands.CreateCustomer;
using CWI.Application.Features.Customers.Commands.DeleteCustomer;
using CWI.Application.Features.Customers.Commands.UpdateCustomer;
using CWI.Application.Features.Customers.Queries.GetCustomerById;
using CWI.Application.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Müşterileri sayfalı listeler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int sortOrder = 1,
        [FromQuery] string? filterCode = null,
        [FromQuery] string? filterName = null,
        [FromQuery] string? filterCity = null,
        [FromQuery] string? filterPhone = null,
        [FromQuery] string? filterEmail = null,
        [FromQuery] string? filterType = null,
        [FromQuery] string? filterStatus = null)
    {
        var query = new GetCustomersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortField = sortField,
            SortOrder = sortOrder,
            FilterCode = filterCode,
            FilterName = filterName,
            FilterCity = filterCity,
            FilterPhone = filterPhone,
            FilterEmail = filterEmail,
            FilterType = filterType,
            FilterStatus = filterStatus
        };
        var result = await _mediator.Send(query);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// ID'ye göre müşteri detaylarını getirir.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Yeni bir müşteri oluşturur.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command);
        var customer = await _mediator.Send(new GetCustomerByIdQuery(result));
        return Ok(new { success = true, data = customer });
    }

    /// <summary>
    /// Mevcut bir müşteriyi günceller.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { success = false, error = "ID mismatch" });

        await _mediator.Send(command);
        
        var customer = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(new { success = true, data = customer });
    }

    /// <summary>
    /// Bir müşteriyi siler (Soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return Ok(new { success = true });
    }
}
