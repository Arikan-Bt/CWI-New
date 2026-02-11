using CWI.Application.Features.Roles.Commands.CreateRole;
using CWI.Application.Features.Roles.Commands.DeleteRole;
using CWI.Application.Features.Roles.Commands.UpdateRole;
using CWI.Application.Features.Roles.Queries.GetAllRoles;
using CWI.Application.Features.Roles.Queries.GetPermissions;
using CWI.Application.Features.Roles.Queries.GetRoleById;
using CWI.Application.Features.Roles.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tüm rolleri listeler (Dropdown için).
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _mediator.Send(new GetAllRolesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Rolleri sayfalı listeler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRoles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int sortOrder = 1,
        [FromQuery] string? filterName = null,
        [FromQuery] string? filterDescription = null,
        [FromQuery] string? filterStatus = null)
    {
        var query = new GetRolesQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = search,
            SortField = sortField,
            SortOrder = sortOrder,
            FilterName = filterName,
            FilterDescription = filterDescription,
            FilterStatus = filterStatus
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// ID'ye göre rol detaylarını getirir.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Tüm yetki listesini gruplandırılmış olarak getirir.
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var result = await _mediator.Send(new GetPermissionsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Yeni bir rol oluşturur.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Mevcut bir rolü günceller.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Bir rolü siler.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        await _mediator.Send(new DeleteRoleCommand(id));
        return NoContent();
    }
}
