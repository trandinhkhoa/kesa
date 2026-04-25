using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Kesa.Controllers.Contracts;

/// <summary>
/// Request contract for creating a candidate profile.
/// </summary>
public sealed class CreateCandidateApiRequest
{
    /// <summary>
    /// Candidate full name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Candidate birth date.
    /// </summary>
    [Required]
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Candidate sex value.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// Candidate dynamic custom fields.
    /// </summary>
    [Required]
    public Dictionary<string, JsonElement> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Optional creator user identifier.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }
}

/// <summary>
/// Request contract for updating a candidate profile.
/// </summary>
public sealed class UpdateCandidateApiRequest
{
    /// <summary>
    /// Candidate full name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Candidate birth date.
    /// </summary>
    [Required]
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Candidate sex value.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// Candidate dynamic custom fields.
    /// </summary>
    [Required]
    public Dictionary<string, JsonElement> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Optional updater user identifier.
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }
}

/// <summary>
/// Response contract for candidate profile endpoints.
/// </summary>
public sealed class CandidateApiResponse
{
    /// <summary>
    /// Candidate identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Candidate full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Candidate birth date.
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Candidate age.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Candidate sex.
    /// </summary>
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// Candidate custom fields.
    /// </summary>
    public Dictionary<string, JsonElement> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Optional creator user identifier.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Optional updater user identifier.
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>
    /// Created timestamp in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response contract for paginated candidate lists.
/// </summary>
public sealed class CandidateListApiResponse
{
    /// <summary>
    /// Candidate page items.
    /// </summary>
    public IReadOnlyList<CandidateApiResponse> Items { get; set; } = [];

    /// <summary>
    /// Total candidate count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 1-based page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; }
}
