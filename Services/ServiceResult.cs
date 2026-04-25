namespace Kesa.Services;

/// <summary>
/// Represents a non-generic service operation result.
/// </summary>
public sealed class ServiceResult
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// Error details when <see cref="IsSuccess"/> is false.
    /// </summary>
    public ServiceError? Error { get; private init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>Successful service result.</returns>
    public static ServiceResult Success() => new() { IsSuccess = true };

    /// <summary>
    /// Creates a failed result with structured error information.
    /// </summary>
    /// <param name="error">Error payload.</param>
    /// <returns>Failed service result.</returns>
    public static ServiceResult Failure(ServiceError error) => new() { IsSuccess = false, Error = error };
}

/// <summary>
/// Represents a typed service operation result.
/// </summary>
/// <typeparam name="T">Success payload type.</typeparam>
public sealed class ServiceResult<T>
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// Success payload value when <see cref="IsSuccess"/> is true.
    /// </summary>
    public T? Value { get; private init; }

    /// <summary>
    /// Error details when <see cref="IsSuccess"/> is false.
    /// </summary>
    public ServiceError? Error { get; private init; }

    /// <summary>
    /// Creates a successful result with payload value.
    /// </summary>
    /// <param name="value">Success payload.</param>
    /// <returns>Successful typed result.</returns>
    public static ServiceResult<T> Success(T value) => new() { IsSuccess = true, Value = value };

    /// <summary>
    /// Creates a failed result with structured error information.
    /// </summary>
    /// <param name="error">Error payload.</param>
    /// <returns>Failed typed result.</returns>
    public static ServiceResult<T> Failure(ServiceError error) => new() { IsSuccess = false, Error = error };
}
