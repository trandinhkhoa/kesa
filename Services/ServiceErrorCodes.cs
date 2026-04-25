namespace Kesa.Services;

/// <summary>
/// Defines stable error code values for service-layer responses.
/// </summary>
public static class ServiceErrorCodes
{
    /// <summary>
    /// Validation failure error code.
    /// </summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>
    /// Resource not found error code.
    /// </summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>
    /// Conflict error code for uniqueness or state conflicts.
    /// </summary>
    public const string Conflict = "CONFLICT";

    /// <summary>
    /// Unexpected failure error code.
    /// </summary>
    public const string Unexpected = "UNEXPECTED_ERROR";
}
