using System.Text.Json;
using Kesa.Models;
using Kesa.Repositories;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// Integration tests for <see cref="CandidateProfileRepository"/> using PostgreSQL testcontainers.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class CandidateProfileRepositoryTests(PostgreSqlContainerFixture fixture)
{
    [Fact]
    public async Task CreateGetAndUpdateAsync_ShouldPersistCoreAndCustomFields()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new CandidateProfileRepository(context);

        var id = Guid.NewGuid();

        var created = await repository.CreateAsync(new CandidateProfile
        {
            Id = id,
            Name = "Alice Nguyen",
            BirthDate = new DateOnly(1998, 5, 10),
            Sex = "Female",
            CustomFields = JsonDocument.Parse("{\"address\":\"Hanoi\"}"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        Assert.Equal(id, created.Id);

        var updated = await repository.UpdateAsync(new CandidateProfile
        {
            Id = id,
            Name = "Alice Tran",
            BirthDate = created.BirthDate,
            Sex = created.Sex,
            CustomFields = JsonDocument.Parse("{\"address\":\"Da Nang\",\"religion\":\"others\"}"),
            CreatedAt = created.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        });

        Assert.NotNull(updated);
        Assert.Equal("Alice Tran", updated!.Name);

        var fetched = await repository.GetByIdAsync(id);
        Assert.NotNull(fetched);
        Assert.Contains("Da Nang", fetched!.CustomFields.RootElement.GetRawText());
    }

    [Fact]
    public async Task ListAsync_ShouldReturnItemsAndTotalCountWithOffsetPagination()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new CandidateProfileRepository(context);

        for (var i = 1; i <= 3; i++)
        {
            await repository.CreateAsync(new CandidateProfile
            {
                Id = Guid.NewGuid(),
                Name = $"Candidate {i}",
                BirthDate = new DateOnly(2000, 1, i),
                Sex = "Other",
                CustomFields = JsonDocument.Parse("{}"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        var page = await repository.ListAsync(pageNumber: 2, pageSize: 2);

        Assert.Equal(3, page.TotalCount);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task DeleteAsync_ShouldUseHardDeleteAndReturnFalseWhenMissing()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new CandidateProfileRepository(context);

        var created = await repository.CreateAsync(new CandidateProfile
        {
            Id = Guid.NewGuid(),
            Name = "Mark Lee",
            BirthDate = new DateOnly(1995, 8, 20),
            Sex = "Male",
            CustomFields = JsonDocument.Parse("{}"),
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

    [Fact]
    public async Task ListAsync_ShouldThrowForInvalidPaginationInputs()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new CandidateProfileRepository(context);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.ListAsync(0, 10));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.ListAsync(1, 0));
    }
}
