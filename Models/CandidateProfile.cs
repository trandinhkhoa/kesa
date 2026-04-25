using System.Text.Json;

namespace Kesa.Models;

/// <summary>
/// Represents a candidate profile with core fields and flexible custom fields.
/// </summary>
public class CandidateProfile
{
    /// <summary>
    /// Unique identifier for the candidate profile.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Candidate full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Candidate date of birth.
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Candidate sex value represented as text.
    /// </summary>
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// JSONB payload storing all dynamic fields defined by administrators.
    /// </summary>
    public JsonDocument CustomFields { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// Optional user id that created this profile.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Optional user id that last updated this profile.
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>
    /// UTC timestamp when the profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the profile was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation to creator user.
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Navigation to updater user.
    /// </summary>
    public User? UpdatedByUser { get; set; }
}
