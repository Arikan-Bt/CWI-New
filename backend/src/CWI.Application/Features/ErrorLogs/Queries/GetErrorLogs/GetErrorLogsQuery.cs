using CWI.Application.DTOs.Common;
using CWI.Application.DTOs.System;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.System;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.ErrorLogs.Queries.GetErrorLogs;

public record GetErrorLogsQuery : IRequest<PagedResult<ErrorLogListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool? IsResolved { get; init; }
    public int? UserId { get; init; }
    public string? RequestUrl { get; init; }
    public string? ExceptionType { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetErrorLogsQueryHandler : IRequestHandler<GetErrorLogsQuery, PagedResult<ErrorLogListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetErrorLogsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ErrorLogListItemDto>> Handle(GetErrorLogsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = _unitOfWork.Repository<ErrorLog, long>()
            .AsQueryable();

        if (request.IsResolved.HasValue)
        {
            query = query.Where(x => x.IsResolved == request.IsResolved.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.RequestUrl))
        {
            query = query.Where(x => x.RequestUrl != null && x.RequestUrl.Contains(request.RequestUrl));
        }

        if (!string.IsNullOrWhiteSpace(request.ExceptionType))
        {
            query = query.Where(x => x.ExceptionType.Contains(request.ExceptionType));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x =>
                x.Message.Contains(request.SearchTerm) ||
                (x.StackTrace != null && x.StackTrace.Contains(request.SearchTerm)) ||
                (x.UserName != null && x.UserName.Contains(request.SearchTerm)) ||
                (x.ErrorCode != null && x.ErrorCode.Contains(request.SearchTerm)));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.OccurredAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.OccurredAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ErrorLogListItemDto
            {
                Id = x.Id,
                OccurredAt = x.OccurredAt,
                IsResolved = x.IsResolved,
                Message = x.Message,
                ExceptionType = x.ExceptionType,
                ErrorCode = x.ErrorCode,
                TraceId = x.TraceId,
                UserId = x.UserId,
                UserName = x.UserName,
                HttpMethod = x.HttpMethod,
                RequestUrl = x.RequestUrl
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ErrorLogListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
