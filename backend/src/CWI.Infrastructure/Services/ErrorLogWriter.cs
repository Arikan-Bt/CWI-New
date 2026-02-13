using CWI.Application.Common.Logging;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.System;
using CWI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CWI.Infrastructure.Services;

public class ErrorLogWriter : IErrorLogWriter
{
    private readonly CWIDbContext _dbContext;

    public ErrorLogWriter(CWIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> WriteAsync(ErrorLogCreateModel model, CancellationToken cancellationToken = default)
    {
        var entity = new ErrorLog
        {
            Message = model.Message,
            StackTrace = model.StackTrace,
            InnerException = model.InnerException,
            Source = model.Source,
            TraceId = model.TraceId,
            UserId = model.UserId,
            UserName = model.UserName,
            IpAddress = model.IpAddress,
            RequestUrl = model.RequestUrl,
            HttpMethod = model.HttpMethod,
            RequestBody = model.RequestBody,
            ExceptionType = model.ExceptionType,
            Target = model.Target,
            RequestQuery = model.RequestQuery,
            RequestRouteValues = model.RequestRouteValues,
            RequestHeaders = model.RequestHeaders,
            RequestContentType = model.RequestContentType,
            RequestContentLength = model.RequestContentLength,
            RequestBodyMasked = model.RequestBodyMasked,
            ErrorCode = model.ErrorCode,
            ParameterName = model.ParameterName,
            Environment = model.Environment,
            MachineName = model.MachineName,
            OccurredAt = model.OccurredAt,
            IsResolved = false
        };

        await _dbContext.ErrorLogs.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task MarkResolvedAsync(long id, int resolvedByUserId, string? note, CancellationToken cancellationToken = default)
    {
        var errorLog = await _dbContext.ErrorLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (errorLog == null)
        {
            throw new KeyNotFoundException($"Error log kaydı bulunamadı. Id: {id}");
        }

        errorLog.IsResolved = true;
        errorLog.ResolutionNote = note;
        errorLog.ResolvedAt = DateTime.UtcNow;
        errorLog.ResolvedByUserId = resolvedByUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
