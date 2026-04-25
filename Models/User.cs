namespace Kesa.Models;

/// <summary>
/// Represents an application user who can create or update candidate profiles.
/// Authentication and authorization are intentionally deferred for this prototype.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password hash placeholder for future authentication support.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User role name (for example: Employee or Admin).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Candidate profiles created by this user.
    /// </summary>
    public ICollection<CandidateProfile> CreatedCandidateProfiles { get; set; } = new List<CandidateProfile>();

    /// <summary>
    /// Candidate profiles updated by this user.
    /// </summary>
    public ICollection<CandidateProfile> UpdatedCandidateProfiles { get; set; } = new List<CandidateProfile>();
}
