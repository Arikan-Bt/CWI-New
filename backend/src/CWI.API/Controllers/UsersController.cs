using CWI.Application.Common.Models;
using CWI.Application.DTOs.Users;
using CWI.Application.Features.Users.Commands.CreateUser;
using CWI.Application.Features.Users.Commands.DeleteUser;
using CWI.Application.Features.Users.Commands.UpdateUser;
using CWI.Application.Features.Users.Queries.GetUserById;
using CWI.Application.Features.Users.Queries.GetUsers;
using CWI.Application.Features.Users.Queries.GetUserActivities;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int sortOrder = 1,
        [FromQuery] string? filterName = null,
        [FromQuery] string? filterEmail = null,
        [FromQuery] string? filterRole = null,
        [FromQuery] string? filterStatus = null)
    {
        var result = await _mediator.Send(new GetUsersQuery 
        { 
            Page = page, 
            PageSize = pageSize, 
            SearchTerm = searchTerm,
            SortField = sortField,
            SortOrder = sortOrder,
            FilterName = filterName,
            FilterEmail = filterEmail,
            FilterRole = filterRole,
            FilterStatus = filterStatus
        });
        
        return Ok(Result<UserPagedResult<UserDto>>.Succeed(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        if (result == null) return NotFound(Result<UserDto>.Failure("User not found."));
        return Ok(Result<UserDto>.Succeed(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var id = await _mediator.Send(new CreateUserCommand(request));
        return Ok(Result<int>.Succeed(id));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id) return BadRequest(Result.Failure("ID mismatch."));
        var result = await _mediator.Send(new UpdateUserCommand(request));
        return Ok(Result<bool>.Succeed(result));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id));
        return Ok(Result<bool>.Succeed(result));
    }

    [HttpGet("{id}/activities")]
    public async Task<IActionResult> GetActivities(int id)
    {
        var result = await _mediator.Send(new GetUserActivitiesQuery(id));
        return Ok(Result<List<UserActivityDto>>.Succeed(result));
    }
}
