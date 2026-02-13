using CWI.Application.DTOs.Common;
using CWI.Application.DTOs.System;
using CWI.Application.Features.ErrorLogs.Commands.ResolveErrorLog;
using CWI.Application.Features.ErrorLogs.Queries.GetErrorLogById;
using CWI.Application.Features.ErrorLogs.Queries.GetErrorLogs;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

/// <summary>
/// Hata log yönetimi API uçları
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class ErrorLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ErrorLogsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Hata loglarını filtreli ve sayfalı şekilde listeler.
    /// </summary>
    /// <param name="filter">Filtreleme ve sayfalama parametreleri</param>
    /// <returns>Filtrelenmiş hata log listesi</returns>
    [HttpGet]
    [Authorize(Policy = Permissions.System.ErrorLogsView)]
    [ProducesResponseType(typeof(PagedResult<ErrorLogListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrorLogs([FromQuery] ErrorLogFilterRequest filter)
    {
        var query = new GetErrorLogsQuery
        {
            Page = filter.Page,
            PageSize = filter.PageSize,
            IsResolved = filter.IsResolved,
            UserId = filter.UserId,
            RequestUrl = filter.RequestUrl,
            ExceptionType = filter.ExceptionType,
            SearchTerm = filter.SearchTerm,
            StartDate = filter.StartDate,
            EndDate = filter.EndDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Belirtilen hata log kaydının detayını getirir.
    /// </summary>
    /// <param name="id">Hata log kaydı kimliği</param>
    /// <returns>Hata log detay bilgisi</returns>
    [HttpGet("{id:long}")]
    [Authorize(Policy = Permissions.System.ErrorLogsView)]
    [ProducesResponseType(typeof(ErrorLogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetErrorLogById(long id)
    {
        var result = await _mediator.Send(new GetErrorLogByIdQuery(id));
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Hata log kaydını çözüldü olarak işaretler.
    /// </summary>
    /// <param name="id">Hata log kaydı kimliği</param>
    /// <param name="request">Çözüm notu bilgisi</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost("{id:long}/resolve")]
    [Authorize(Policy = Permissions.System.ErrorLogsResolve)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResolveErrorLog(long id, [FromBody] ResolveErrorLogRequest request)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new ResolveErrorLogCommand(id, _currentUserService.UserId.Value, request.ResolutionNote);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
