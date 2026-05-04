using Kesa.Data;
using Kesa.Models;
using Microsoft.EntityFrameworkCore;

namespace Kesa.Repositories;

/// <summary>
/// EF Core repository for profile field definition persistence operations.
/// </summary>
public sealed class DefaultFieldsRepository(KesaDbContext dbContext) : IDefaultFieldsRepository
{
    /// <inheritdoc />
    public async Task<DefaultFields> CreateAsync(
        DefaultFields fieldDefinition,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.DefaultFields.Add(fieldDefinition);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return fieldDefinition;
    }

    /// <inheritdoc />
    public Task<DefaultFields?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DefaultFields
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DefaultFields?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DefaultFields
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DefaultFields>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.DefaultFields
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Key)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DefaultFields?> UpdateAsync(
        DefaultFields fieldDefinition,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.DefaultFields
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
        var existing = await dbContext.DefaultFields
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.DefaultFields.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }
}
