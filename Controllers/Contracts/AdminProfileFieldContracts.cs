using System.ComponentModel.DataAnnotations;

namespace Kesa.Controllers.Contracts;

/// <summary>
/// Request contract for creating an admin profile field definition.
/// </summary>
public sealed class CreateProfileFieldDefinitionApiRequest
{
    /// <summary>
    /// Field display name.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stable field key used by candidate custom fields.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Data type for this field.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates whether this field is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Enum options when <see cref="DataType"/> is Enum.
    /// </summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>
    /// Optional creator user identifier.
    /// </summary>
    public Guid? CreatedBy { get; set; }
}

/// <summary>
/// Request contract for updating an admin profile field definition.
/// </summary>
public sealed class UpdateProfileFieldDefinitionApiRequest
{
    /// <summary>
    /// Field display name.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stable field key used by candidate custom fields.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Data type for this field.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates whether this field is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Enum options when <see cref="DataType"/> is Enum.
    /// </summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>
    /// Optional updater user identifier.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Response contract for profile field definition endpoints.
/// </summary>
public sealed class ProfileFieldDefinitionApiResponse
{
    /// <summary>
    /// Field definition identifier.
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
    /// Data type for this field.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates whether this field is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Enum options for Enum fields.
    /// </summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>
    /// Optional creator user identifier.
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
