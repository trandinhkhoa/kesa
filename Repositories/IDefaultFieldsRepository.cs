using Kesa.Models;

namespace Kesa.Repositories;

/// <summary>
/// Defines data-access operations for profile field definitions.
/// </summary>
public interface IDefaultFieldsRepository
{
    /// <summary>
    /// Creates a new profile field definition.
    /// </summary>
    /// <param name="fieldDefinition">Field definition entity to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created field definition.</returns>
    Task<DefaultFields> CreateAsync(
        DefaultFields fieldDefinition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a profile field definition by its unique identifier.
    /// </summary>
    /// <param name="id">Field definition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The field definition when found; otherwise null.</returns>
    Task<DefaultFields?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a profile field definition by its unique key.
    /// </summary>
    /// <param name="key">Field definition key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The field definition when found; otherwise null.</returns>
    Task<DefaultFields?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all profile field definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All field definitions in ascending name order.</returns>
    Task<IReadOnlyList<DefaultFields>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing profile field definition.
    /// </summary>
    /// <param name="fieldDefinition">Field definition with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated field definition when found; otherwise null.</returns>
    Task<DefaultFields?> UpdateAsync(
        DefaultFields fieldDefinition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing profile field definition.
    /// </summary>
    /// <param name="id">Field definition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when deletion occurred; otherwise false for missing record.</returns>
    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
