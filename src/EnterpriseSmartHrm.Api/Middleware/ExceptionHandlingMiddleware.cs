using System.Net;
using System.Text.Json;
using EnterpriseSmartHrm.Application.Common.Exceptions;
using EnterpriseSmartHrm.Application.Contracts.Common;
using FluentValidation;

namespace EnterpriseSmartHrm.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var statusCode = GetStatusCode(exception);
        var message = GetMessage(exception, statusCode);
        var errors = GetErrors(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with {StatusCode}. TraceId: {TraceId}", (int)statusCode, traceId);
        }

        var response = ApiResponse<object>.Fail(message, errors, traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private HttpStatusCode GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            AppException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

    private string GetMessage(Exception exception, HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.InternalServerError && !_environment.IsDevelopment())
        {
            return "An unexpected error occurred.";
        }

        return exception.Message;
    }

    private static IReadOnlyList<string> GetErrors(Exception exception) =>
        exception switch
        {
            ValidationException validationException => validationException.Errors
                .Select(error => error.ErrorMessage)
                .Distinct()
                .ToArray(),
            _ => Array.Empty<string>()
        };
}
