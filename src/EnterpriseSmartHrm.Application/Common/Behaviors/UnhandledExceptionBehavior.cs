using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseSmartHrm.Application.Common.Behaviors;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(
                exception,
                "Unhandled exception for request {RequestName}. Request: {@Request}",
                requestName,
                request);

            throw;
        }
    }
}
