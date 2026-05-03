namespace Kesa.Models;

/// <summary>
/// Defines metadata and validation rules for dynamic candidate profile fields stored in JSONB.
/// </summary>
public class DefaultFields
{
    /// <summary>
    /// Unique identifier for the field definition.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable display name of the field.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stable key used by API clients in customFields payloads.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Data type descriptor (for example: String, Number, Date, Boolean, Enum).
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this field must be present during candidate write operations.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates whether this field is currently active and available for use.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// JSON string containing valid options when DataType is Enum.
    /// </summary>
    public string? OptionsJson { get; set; }

    /// <summary>
    /// Optional creator user id for auditability.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// UTC timestamp when the field definition was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the field definition was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
