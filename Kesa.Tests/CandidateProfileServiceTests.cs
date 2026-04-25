using System.Text.Json;
using Kesa.Repositories;
using Kesa.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// Integration tests for <see cref="CandidateProfileService"/>.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class CandidateProfileServiceTests(PostgreSqlContainerFixture fixture)
{
    [Fact]
    public async Task CreateAsync_ShouldValidateUnknownCustomFieldKeys()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new ProfileFieldDefinitionRepository(context);

        var service = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var result = await service.CreateAsync(new CreateCandidateProfileRequest
        {
            Name = "Jade Pham",
            BirthDate = new DateOnly(1994, 8, 15),
            Sex = "Female",
            CustomFields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            {
                ["unknownField"] = JsonDocument.Parse("\"value\"").RootElement.Clone()
            }
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.ValidationError, result.Error!.Code);
        Assert.NotNull(result.Error.ValidationErrors);
        Assert.Contains("customFields.unknownField", result.Error.ValidationErrors!.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAndGetAsync_ShouldEnforceRequiredFieldsAndDeriveAge()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new ProfileFieldDefinitionRepository(context);

        var fieldService = new ProfileFieldDefinitionService(fieldDefinitionRepository, NullLogger<ProfileFieldDefinitionService>.Instance);
        var candidateService = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var fieldCreate = await fieldService.CreateAsync(new CreateProfileFieldDefinitionRequest
        {
            Name = "Nationality",
            Key = "nationality",
            DataType = "String",
            IsRequired = true,
            IsActive = true
        });

        Assert.True(fieldCreate.IsSuccess);

        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-25);

        var createResult = await candidateService.CreateAsync(new CreateCandidateProfileRequest
        {
            Name = "Dylan Ho",
            BirthDate = birthDate,
            Sex = "Male",
            CustomFields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            {
                ["nationality"] = JsonDocument.Parse("\"Vietnamese\"").RootElement.Clone()
            }
        });

        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Value);

        var getResult = await candidateService.GetByIdAsync(createResult.Value!.Id);

        Assert.True(getResult.IsSuccess);
        Assert.NotNull(getResult.Value);
        Assert.Equal(25, getResult.Value!.Age);
    }

    [Fact]
    public async Task ListAsync_ShouldMapInvalidPaginationToValidationError()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new ProfileFieldDefinitionRepository(context);

        var service = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var result = await service.ListAsync(0, 10);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.ValidationError, result.Error!.Code);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFoundWhenCandidateMissing()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new ProfileFieldDefinitionRepository(context);

        var service = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.NotFound, result.Error!.Code);
    }
}
