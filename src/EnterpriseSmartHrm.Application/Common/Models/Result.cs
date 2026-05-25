namespace EnterpriseSmartHrm.Application.Common.Models;

public enum ResultStatus
{
    Success = 0,
    Failure = 1,
    ValidationError = 2,
    NotFound = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Conflict = 6
}

public class Result
{
    protected Result(bool isSuccess, string message, ResultStatus status, IReadOnlyList<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Status = status;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string Message { get; }

    public ResultStatus Status { get; }

    public IReadOnlyList<string> Errors { get; }

    public static Result Success(string message = "Operation completed successfully.") =>
        new(true, message, ResultStatus.Success);

    public static Result Failure(string message = "Operation failed.", IReadOnlyList<string>? errors = null) =>
        new(false, message, ResultStatus.Failure, errors);

    public static Result ValidationFailure(IReadOnlyList<string> errors, string message = "Validation failed.") =>
        new(false, message, ResultStatus.ValidationError, errors);

    public static Result NotFound(string message = "Resource was not found.") =>
        new(false, message, ResultStatus.NotFound);

    public static Result Unauthorized(string message = "Authentication is required.") =>
        new(false, message, ResultStatus.Unauthorized);

    public static Result Forbidden(string message = "You do not have permission to perform this action.") =>
        new(false, message, ResultStatus.Forbidden);

    public static Result Conflict(string message = "Request conflicts with the current state.") =>
        new(false, message, ResultStatus.Conflict);
}

public class Result<T> : Result
{
    private Result(T? value, bool isSuccess, string message, ResultStatus status, IReadOnlyList<string>? errors = null)
        : base(isSuccess, message, status, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value, string message = "Operation completed successfully.") =>
        new(value, true, message, ResultStatus.Success);

    public static new Result<T> Failure(string message = "Operation failed.", IReadOnlyList<string>? errors = null) =>
        new(default, false, message, ResultStatus.Failure, errors);

    public static new Result<T> ValidationFailure(IReadOnlyList<string> errors, string message = "Validation failed.") =>
        new(default, false, message, ResultStatus.ValidationError, errors);

    public static new Result<T> NotFound(string message = "Resource was not found.") =>
        new(default, false, message, ResultStatus.NotFound);

    public static new Result<T> Unauthorized(string message = "Authentication is required.") =>
        new(default, false, message, ResultStatus.Unauthorized);

    public static new Result<T> Forbidden(string message = "You do not have permission to perform this action.") =>
        new(default, false, message, ResultStatus.Forbidden);

    public static new Result<T> Conflict(string message = "Request conflicts with the current state.") =>
        new(default, false, message, ResultStatus.Conflict);
}
