using Kesa.Models;
using Kesa.Repositories;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// Integration tests for <see cref="ProfileFieldDefinitionRepository"/> using PostgreSQL testcontainers.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class ProfileFieldDefinitionRepositoryTests(PostgreSqlContainerFixture fixture)
{
    [Fact]
    public async Task CreateAndGetByIdAsync_ShouldPersistAndReturnEntity()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new ProfileFieldDefinitionRepository(context);

        var entity = new ProfileFieldDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Language",
            Key = "language",
            DataType = "String",
            IsRequired = false,
            IsActive = true,
            OptionsJson = null,
            CreatedBy = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await repository.CreateAsync(entity);
        var fetched = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal("language", fetched!.Key);
        Assert.Equal("String", fetched.DataType);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNullForMissingEntity()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new ProfileFieldDefinitionRepository(context);

        var updated = await repository.UpdateAsync(new ProfileFieldDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Unknown",
            Key = "unknown",
            DataType = "String",
            IsRequired = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        Assert.Null(updated);
    }

    [Fact]
    public async Task DeleteAsync_ShouldUseHardDeleteAndReportMissingRows()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new ProfileFieldDefinitionRepository(context);

        var created = await repository.CreateAsync(new ProfileFieldDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Certification",
            Key = "certification",
            DataType = "String",
            IsRequired = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var deleted = await repository.DeleteAsync(created.Id);
        var deletedAgain = await repository.DeleteAsync(created.Id);
        var fetched = await repository.GetByIdAsync(created.Id);

        Assert.True(deleted);
        Assert.False(deletedAgain);
        Assert.Null(fetched);
    }
}
