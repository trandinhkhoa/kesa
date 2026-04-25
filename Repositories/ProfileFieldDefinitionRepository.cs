using Kesa.Data;
using Kesa.Models;
using Microsoft.EntityFrameworkCore;

namespace Kesa.Repositories;

/// <summary>
/// EF Core repository for profile field definition persistence operations.
/// </summary>
public sealed class ProfileFieldDefinitionRepository(KesaDbContext dbContext) : IProfileFieldDefinitionRepository
{
    /// <inheritdoc />
    public async Task<ProfileFieldDefinition> CreateAsync(
        ProfileFieldDefinition fieldDefinition,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.ProfileFieldDefinitions.Add(fieldDefinition);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return fieldDefinition;
    }

    /// <inheritdoc />
    public Task<ProfileFieldDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProfileFieldDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProfileFieldDefinition?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProfileFieldDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfileFieldDefinition>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ProfileFieldDefinitions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Key)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProfileFieldDefinition?> UpdateAsync(
        ProfileFieldDefinition fieldDefinition,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.ProfileFieldDefinitions
            .SingleOrDefaultAsync(x => x.Id == fieldDefinition.Id, cancellationToken);

        if (existing is null)
        {
            return null;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        existing.Name = fieldDefinition.Name;
        existing.Key = fieldDefinition.Key;
        existing.DataType = fieldDefinition.DataType;
        existing.IsRequired = fieldDefinition.IsRequired;
        existing.IsActive = fieldDefinition.IsActive;
        existing.OptionsJson = fieldDefinition.OptionsJson;
        existing.CreatedBy = fieldDefinition.CreatedBy;
        existing.CreatedAt = fieldDefinition.CreatedAt;
        existing.UpdatedAt = fieldDefinition.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.ProfileFieldDefinitions
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.ProfileFieldDefinitions.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }
}
