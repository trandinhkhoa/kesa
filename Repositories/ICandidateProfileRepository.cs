using Kesa.Models;

namespace Kesa.Repositories;

/// <summary>
/// Defines data-access operations for candidate profiles.
/// </summary>
public interface ICandidateProfileRepository
{
    /// <summary>
    /// Creates a new candidate profile.
    /// </summary>
    /// <param name="candidateProfile">Candidate profile entity to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created candidate profile.</returns>
    Task<CandidateProfile> CreateAsync(
        CandidateProfile candidateProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a candidate profile by its unique identifier.
    /// </summary>
    /// <param name="id">Candidate profile identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The candidate profile when found; otherwise null.</returns>
    Task<CandidateProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists candidate profiles with offset pagination and total count.
    /// </summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Page size greater than zero.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing page items and total row count.</returns>
    Task<(IReadOnlyList<CandidateProfile> Items, int TotalCount)> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing candidate profile.
    /// </summary>
    /// <param name="candidateProfile">Candidate profile with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated candidate profile when found; otherwise null.</returns>
    Task<CandidateProfile?> UpdateAsync(
        CandidateProfile candidateProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing candidate profile using hard delete semantics.
    /// </summary>
    /// <param name="id">Candidate profile identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when deletion occurred; otherwise false for missing record.</returns>
    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
