namespace BusinessObjects.DTOs;

public class ServiceResult
{
    protected ServiceResult(bool isSuccess, string message, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string Message { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ServiceResult Success(string message = "")
    {
        return new ServiceResult(true, message, Array.Empty<string>());
    }

    public static ServiceResult Failure(string message, params string[] errors)
    {
        return new ServiceResult(false, message, NormalizeErrors(errors, message));
    }

    protected static IReadOnlyList<string> NormalizeErrors(IEnumerable<string?> errors, string message)
    {
        var normalizedErrors = errors
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .Select(error => error!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedErrors.Length > 0)
        {
            return normalizedErrors;
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            return [message.Trim()];
        }

        return Array.Empty<string>();
    }
}

public sealed class ServiceResult<T> : ServiceResult
{
    private ServiceResult(bool isSuccess, T? data, string message, IReadOnlyList<string> errors)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public T? Data { get; }

    public static ServiceResult<T> Success(T? data, string message = "")
    {
        return new ServiceResult<T>(true, data, message, Array.Empty<string>());
    }

    public new static ServiceResult<T> Failure(string message, params string[] errors)
    {
        return new ServiceResult<T>(false, default, message, NormalizeErrors(errors, message));
    }
}
