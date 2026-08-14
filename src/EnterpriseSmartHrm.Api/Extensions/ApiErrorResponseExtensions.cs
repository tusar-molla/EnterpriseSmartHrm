using EnterpriseSmartHrm.Application.Common.Models;
using System.Text.Json;

namespace EnterpriseSmartHrm.Api.Extensions;

public static class ApiErrorResponseExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static Task WriteApiErrorAsync(
        this HttpContext context,
        int statusCode,
        string message,
        IReadOnlyList<string>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        var response = ApiResponse<object>.Fail(message, errors, context.TraceIdentifier);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }
}
