using System.Text.Json;

namespace Kesa.Services;

/// <summary>
/// Request DTO for creating a candidate profile.
/// </summary>
public sealed class CreateCandidateProfileRequest
{
    /// <summary>
    /// Candidate full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Candidate birth date.
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Candidate sex value.
    /// </summary>
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// Dynamic custom field values keyed by field definition key.
    /// </summary>
    public Dictionary<string, JsonElement> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Optional creator user id.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }
}

/// <summary>
/// Request DTO for updating a candidate profile.
/// </summary>
public sealed class UpdateCandidateProfileRequest
{
    /// <summary>
    /// Candidate full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Candidate birth date.
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Candidate sex value.
    /// </summary>
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// Dynamic custom field values keyed by field definition key.
    /// </summary>
    public Dictionary<string, JsonElement> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Optional updater user id.
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }
}

/// <summary>
/// Response DTO for candidate profile data.
/// </summary>
public sealed class CandidateProfileResponse
{
    /// <summary>
    /// Candidate profile identifier.
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
    /// Derived candidate age in years.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Candidate sex value.
    /// </summary>
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// Dynamic custom field values.
    /// </summary>
    public Dictionary<string, JsonElement> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Optional creator user id.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Optional updater user id.
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
/// Response DTO for paginated candidate list results.
/// </summary>
public sealed class CandidateProfileListResponse
{
    /// <summary>
    /// List items for the requested page.
    /// </summary>
    public IReadOnlyList<CandidateProfileResponse> Items { get; set; } = [];

    /// <summary>
    /// Total matching candidate count.
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
