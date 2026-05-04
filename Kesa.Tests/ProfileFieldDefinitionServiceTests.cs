using Kesa.Repositories;
using Kesa.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// Integration tests for <see cref="ProfileFieldDefinitionService"/>.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class ProfileFieldDefinitionServiceTests(PostgreSqlContainerFixture fixture)
{
    [Fact]
    public async Task CreateAsync_ShouldReturnValidationErrorForUnsupportedDataType()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new DefaultFieldsRepository(context);
        var service = new ProfileFieldDefinitionService(repository, NullLogger<ProfileFieldDefinitionService>.Instance);

        var result = await service.CreateAsync(new CreateProfileFieldDefinitionRequest
        {
            Name = "Level",
            Key = "level",
            DataType = "Object",
            IsRequired = false,
            IsActive = true
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.ValidationError, result.Error!.Code);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnConflictWhenKeyAlreadyExists()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new DefaultFieldsRepository(context);
        var service = new ProfileFieldDefinitionService(repository, NullLogger<ProfileFieldDefinitionService>.Instance);

        var first = await service.CreateAsync(new CreateProfileFieldDefinitionRequest
        {
            Name = "Certification",
            Key = "certification",
            DataType = "String",
            IsRequired = false,
            IsActive = true
        });

        Assert.True(first.IsSuccess);

        var second = await service.CreateAsync(new CreateProfileFieldDefinitionRequest
        {
            Name = "Certification Duplicate",
            Key = "certification",
            DataType = "String",
            IsRequired = false,
            IsActive = true
        });

        Assert.False(second.IsSuccess);
        Assert.NotNull(second.Error);
        Assert.Equal(ServiceErrorCodes.Conflict, second.Error!.Code);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFoundForMissingDefinition()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var repository = new DefaultFieldsRepository(context);
        var service = new ProfileFieldDefinitionService(repository, NullLogger<ProfileFieldDefinitionService>.Instance);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.NotFound, result.Error!.Code);
    }
}
