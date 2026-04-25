using System.Text.Json;
using Kesa.Data;
using Kesa.Models;
using Microsoft.EntityFrameworkCore;

namespace Kesa.Repositories;

/// <summary>
/// EF Core repository for candidate profile persistence operations.
/// </summary>
public sealed class CandidateProfileRepository(KesaDbContext dbContext) : ICandidateProfileRepository
{
    private const int MinimumPageNumber = 1;
    private const int MinimumPageSize = 1;

    /// <inheritdoc />
    public async Task<CandidateProfile> CreateAsync(
        CandidateProfile candidateProfile,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var entity = CloneForWrite(candidateProfile);

        dbContext.CandidateProfiles.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public Task<CandidateProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<CandidateProfile> Items, int TotalCount)> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < MinimumPageNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "pageNumber must be greater than or equal to 1.");
        }

        if (pageSize < MinimumPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "pageSize must be greater than or equal to 1.");
        }

        var baseQuery = dbContext.CandidateProfiles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<CandidateProfile?> UpdateAsync(
        CandidateProfile candidateProfile,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.CandidateProfiles
            .SingleOrDefaultAsync(x => x.Id == candidateProfile.Id, cancellationToken);

        if (existing is null)
        {
            return null;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        existing.Name = candidateProfile.Name;
        existing.BirthDate = candidateProfile.BirthDate;
        existing.Sex = candidateProfile.Sex;
        existing.CustomFields = CloneJsonDocument(candidateProfile.CustomFields);
        existing.CreatedByUserId = candidateProfile.CreatedByUserId;
        existing.UpdatedByUserId = candidateProfile.UpdatedByUserId;
        existing.CreatedAt = candidateProfile.CreatedAt;
        existing.UpdatedAt = candidateProfile.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.CandidateProfiles
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.CandidateProfiles.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    private static CandidateProfile CloneForWrite(CandidateProfile candidateProfile)
    {
        return new CandidateProfile
        {
            Id = candidateProfile.Id,
            Name = candidateProfile.Name,
            BirthDate = candidateProfile.BirthDate,
            Sex = candidateProfile.Sex,
            CustomFields = CloneJsonDocument(candidateProfile.CustomFields),
            CreatedByUserId = candidateProfile.CreatedByUserId,
            UpdatedByUserId = candidateProfile.UpdatedByUserId,
            CreatedAt = candidateProfile.CreatedAt,
            UpdatedAt = candidateProfile.UpdatedAt
        };
    }

    private static JsonDocument CloneJsonDocument(JsonDocument jsonDocument)
    {
        return JsonDocument.Parse(jsonDocument.RootElement.GetRawText());
    }
}
