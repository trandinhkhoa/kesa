namespace Kesa.Services;

/// <summary>
/// Exposes business operations for candidate profiles.
/// </summary>
public interface ICandidateProfileService
{
    /// <summary>
    /// Creates a candidate profile with service-layer validation.
    /// </summary>
    /// <param name="request">Create request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing created candidate data.</returns>
    Task<ServiceResult<CandidateProfileResponse>> CreateAsync(
        CreateCandidateProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a candidate profile by id.
    /// </summary>
    /// <param name="id">Candidate identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing candidate data when found.</returns>
    Task<ServiceResult<CandidateProfileResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists candidate profiles with pagination.
    /// </summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Requested page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing paginated candidate data.</returns>
    Task<ServiceResult<CandidateProfileListResponse>> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing candidate profile.
    /// </summary>
    /// <param name="id">Candidate identifier.</param>
    /// <param name="request">Update request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing updated candidate data.</returns>
    Task<ServiceResult<CandidateProfileResponse>> UpdateAsync(
        Guid id,
        UpdateCandidateProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing candidate profile.
    /// </summary>
    /// <param name="id">Candidate identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result indicating success or not-found.</returns>
    Task<ServiceResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
