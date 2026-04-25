namespace Kesa.Services;

/// <summary>
/// Represents a structured service-layer error payload.
/// </summary>
/// <param name="Code">Stable service error code.</param>
/// <param name="Message">Human-readable error message.</param>
/// <param name="ValidationErrors">Optional field-level validation errors.</param>
public sealed record ServiceError(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);
