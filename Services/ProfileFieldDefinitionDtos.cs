namespace Kesa.Services;

/// <summary>
/// Request DTO for creating a profile field definition.
/// </summary>
public sealed class CreateProfileFieldDefinitionRequest
{
    /// <summary>
    /// Field display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stable field key used in candidate custom fields.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Data type name (String, Number, Date, Boolean, Enum).
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this field is required for candidate writes.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates whether this field is active for validation.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Enum options used when <see cref="DataType"/> is Enum.
    /// </summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>
    /// Optional creator user id.
    /// </summary>
    public Guid? CreatedBy { get; set; }
}

/// <summary>
/// Request DTO for updating a profile field definition.
/// </summary>
public sealed class UpdateProfileFieldDefinitionRequest
{
    /// <summary>
    /// Field display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stable field key used in candidate custom fields.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Data type name (String, Number, Date, Boolean, Enum).
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this field is required for candidate writes.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates whether this field is active for validation.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Enum options used when <see cref="DataType"/> is Enum.
    /// </summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>
    /// Optional updater user id for audit metadata.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Response DTO for profile field definition data.
/// </summary>
public sealed class ProfileFieldDefinitionResponse
{
    /// <summary>
    /// Field identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Field display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stable field key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Data type name.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Required flag.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Active flag.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Enum options for Enum field types.
    /// </summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>
    /// Creator user id.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Created timestamp in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
