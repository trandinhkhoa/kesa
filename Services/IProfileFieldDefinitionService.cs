namespace Kesa.Services;

/// <summary>
/// Exposes business operations for profile field definitions.
/// </summary>
public interface IProfileFieldDefinitionService
{
    /// <summary>
    /// Creates a profile field definition with service-layer validation.
    /// </summary>
    /// <param name="request">Create request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing created definition data.</returns>
    Task<ServiceResult<ProfileFieldDefinitionResponse>> CreateAsync(
        CreateProfileFieldDefinitionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a profile field definition by id.
    /// </summary>
    /// <param name="id">Definition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing definition data when found.</returns>
    Task<ServiceResult<ProfileFieldDefinitionResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all profile field definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing all definition items.</returns>
    Task<ServiceResult<IReadOnlyList<ProfileFieldDefinitionResponse>>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a profile field definition with service-layer validation.
    /// </summary>
    /// <param name="id">Definition identifier.</param>
    /// <param name="request">Update request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result containing updated definition data.</returns>
    Task<ServiceResult<ProfileFieldDefinitionResponse>> UpdateAsync(
        Guid id,
        UpdateProfileFieldDefinitionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a profile field definition.
    /// </summary>
    /// <param name="id">Definition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result indicating success or not-found.</returns>
    Task<ServiceResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
