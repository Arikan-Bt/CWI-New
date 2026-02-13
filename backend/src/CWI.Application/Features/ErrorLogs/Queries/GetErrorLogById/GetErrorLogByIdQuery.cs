using CWI.Application.DTOs.System;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.System;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.ErrorLogs.Queries.GetErrorLogById;

public record GetErrorLogByIdQuery(long Id) : IRequest<ErrorLogDetailDto?>;

public class GetErrorLogByIdQueryHandler : IRequestHandler<GetErrorLogByIdQuery, ErrorLogDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetErrorLogByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorLogDetailDto?> Handle(GetErrorLogByIdQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<ErrorLog, long>()
            .AsQueryable()
            .Where(x => x.Id == request.Id)
            .Select(x => new ErrorLogDetailDto
            {
                Id = x.Id,
                Message = x.Message,
                StackTrace = x.StackTrace,
                InnerException = x.InnerException,
                Source = x.Source,
                TraceId = x.TraceId,
                UserId = x.UserId,
                UserName = x.UserName,
                IpAddress = x.IpAddress,
                RequestUrl = x.RequestUrl,
                HttpMethod = x.HttpMethod,
                ExceptionType = x.ExceptionType,
                Target = x.Target,
                RequestQuery = x.RequestQuery,
                RequestRouteValues = x.RequestRouteValues,
                RequestHeaders = x.RequestHeaders,
                RequestContentType = x.RequestContentType,
                RequestContentLength = x.RequestContentLength,
                RequestBodyMasked = x.RequestBodyMasked,
                ErrorCode = x.ErrorCode,
                ParameterName = x.ParameterName,
                Environment = x.Environment,
                MachineName = x.MachineName,
                OccurredAt = x.OccurredAt,
                IsResolved = x.IsResolved,
                ResolutionNote = x.ResolutionNote,
                ResolvedAt = x.ResolvedAt,
                ResolvedByUserId = x.ResolvedByUserId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
