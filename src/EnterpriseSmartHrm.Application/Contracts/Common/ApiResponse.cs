namespace EnterpriseSmartHrm.Application.Contracts.Common;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T? data, string message = "Operation completed successfully.", string? traceId = null) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            TraceId = traceId
        };

    public static ApiResponse<T> Fail(
        string message = "Operation failed.",
        IReadOnlyList<string>? errors = null,
        string? traceId = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? Array.Empty<string>(),
            TraceId = traceId
        };
}
