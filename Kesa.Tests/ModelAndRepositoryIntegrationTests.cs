using System.Text.Json;
using Kesa.Models;
using Kesa.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// Additional integration tests covering model constraints and repository relational behavior.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class ModelAndRepositoryIntegrationTests(PostgreSqlContainerFixture fixture)
{
    [Fact]
    public async Task ProfileFieldDefinition_KeyShouldBeUniqueAtDatabaseLevel()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new DefaultFieldsRepository(context);

        await repository.CreateAsync(new DefaultFields
        {
            Id = Guid.NewGuid(),
            Name = "Office",
            Key = "office",
            DataType = "String",
            IsRequired = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.CreateAsync(new DefaultFields
        {
            Id = Guid.NewGuid(),
            Name = "Office Duplicate",
            Key = "office",
            DataType = "String",
            IsRequired = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }));
    }

    [Fact]
    public async Task CandidateProfile_ForeignKeysShouldSetNullWhenUserIsDeleted()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        var userId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();

        await using var context = fixture.CreateDbContext();
        context.Users.Add(new User
        {
            Id = userId,
            Email = "candidate-owner@example.com",
            PasswordHash = "hash",
            Role = "Employee",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var candidateRepository = new CandidateProfileRepository(context);
        await context.SaveChangesAsync();

        await candidateRepository.CreateAsync(new CandidateProfile
        {
            Id = candidateId,
            Name = "Relational Test",
            BirthDate = new DateOnly(1990, 1, 1),
            Sex = "Other",
            CustomFields = JsonDocument.Parse("{}"),
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var owner = await context.Users.SingleAsync(x => x.Id == userId);
        context.Users.Remove(owner);
        await context.SaveChangesAsync();

        var candidate = await candidateRepository.GetByIdAsync(candidateId);

        Assert.NotNull(candidate);
        Assert.Null(candidate!.CreatedByUserId);
        Assert.Null(candidate.UpdatedByUserId);
    }
}
