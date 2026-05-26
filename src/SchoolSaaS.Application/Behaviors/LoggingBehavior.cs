using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next(cancellationToken);

        stopwatch.Stop();

        if (response.IsFailure)
        {
            logger.LogWarning(
                "{RequestName} failed in {ElapsedMs}ms with errors: {ErrorCodes}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                string.Join(", ", response.Errors.Select(e => e.Code)));
        }
        else
        {
            logger.LogInformation(
                "{RequestName} completed in {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
