using CWI.Application.Features.Dashboard.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanıcı rolüne göre dashboard tasarımını getirir.
    /// </summary>
    /// <returns>Dashboard widget listesi</returns>
    [HttpGet]
    public async Task<ActionResult<DashboardViewModel>> GetDashboard()
    {
        // I will trust the Query Handler to determine the role via CurrentUserService
        return Ok(await _mediator.Send(new GetDashboardQuery()));
    }
}
