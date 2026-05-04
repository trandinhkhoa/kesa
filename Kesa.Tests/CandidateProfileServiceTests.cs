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
    public async Task CreateAsync_ShouldReturnValidationErrorsForMissingCoreColumns()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

        var service = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var result = await service.CreateAsync(new CreateCandidateProfileRequest
        {
            Name = string.Empty,
            BirthDate = default,
            Sex = string.Empty,
            CustomFields = new Dictionary<string, JsonElement>()
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.ValidationError, result.Error!.Code);
        Assert.NotNull(result.Error.ValidationErrors);
        Assert.Contains("name", result.Error.ValidationErrors!.Keys);
        Assert.Contains("birthDate", result.Error.ValidationErrors.Keys);
        Assert.Contains("sex", result.Error.ValidationErrors.Keys);
    }

    [Fact]
    public async Task CreateAsync_ShouldAllowAdHocCustomFieldKeys()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

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
                ["adhocField"] = JsonDocument.Parse("\"ad-hoc value\"").RootElement.Clone()
            }
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var getResult = await service.GetByIdAsync(result.Value!.Id);
        Assert.True(getResult.IsSuccess);
        Assert.NotNull(getResult.Value);
        Assert.True(getResult.Value!.CustomFields.TryGetValue("adhocField", out var persistedValue));
        Assert.Equal("ad-hoc value", persistedValue.GetString());
    }

    [Fact]
    public async Task CreateAndGetAsync_ShouldEnforceRequiredFieldsAndDeriveAge()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

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
    public async Task CreateAsync_ShouldRejectInvalidDynamicFieldDataType()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

        var fieldService = new ProfileFieldDefinitionService(fieldDefinitionRepository, NullLogger<ProfileFieldDefinitionService>.Instance);
        var candidateService = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var fieldCreate = await fieldService.CreateAsync(new CreateProfileFieldDefinitionRequest
        {
            Name = "ExperienceYears",
            Key = "experienceYears",
            DataType = "Number",
            IsRequired = false,
            IsActive = true
        });
        Assert.True(fieldCreate.IsSuccess);

        var result = await candidateService.CreateAsync(new CreateCandidateProfileRequest
        {
            Name = "Type Mismatch",
            BirthDate = new DateOnly(1992, 2, 2),
            Sex = "Other",
            CustomFields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            {
                ["experienceYears"] = JsonDocument.Parse("\"five\"").RootElement.Clone()
            }
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.ValidationError, result.Error!.Code);
        Assert.Contains("customFields.experienceYears", result.Error.ValidationErrors!.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectEnumValueOutsideAllowedOptions()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

        var fieldService = new ProfileFieldDefinitionService(fieldDefinitionRepository, NullLogger<ProfileFieldDefinitionService>.Instance);
        var candidateService = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var fieldCreate = await fieldService.CreateAsync(new CreateProfileFieldDefinitionRequest
        {
            Name = "EmploymentType",
            Key = "employmentType",
            DataType = "Enum",
            IsRequired = false,
            IsActive = true,
            Options = ["full_time", "contract"]
        });
        Assert.True(fieldCreate.IsSuccess);

        var result = await candidateService.CreateAsync(new CreateCandidateProfileRequest
        {
            Name = "Enum Mismatch",
            BirthDate = new DateOnly(1991, 1, 1),
            Sex = "Male",
            CustomFields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            {
                ["employmentType"] = JsonDocument.Parse("\"intern\"").RootElement.Clone()
            }
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrorCodes.ValidationError, result.Error!.Code);
        Assert.Contains("customFields.employmentType", result.Error.ValidationErrors!.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldDeriveAgeCorrectlyBeforeBirthday()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

        var candidateService = new CandidateProfileService(
            candidateRepository,
            fieldDefinitionRepository,
            NullLogger<CandidateProfileService>.Instance);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcomingBirthday = today.AddDays(1);
        var birthDate = new DateOnly(today.Year - 30, upcomingBirthday.Month, upcomingBirthday.Day);

        var created = await candidateService.CreateAsync(new CreateCandidateProfileRequest
        {
            Name = "Age Edge",
            BirthDate = birthDate,
            Sex = "Female",
            CustomFields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
        });

        Assert.True(created.IsSuccess);
        var getResult = await candidateService.GetByIdAsync(created.Value!.Id);

        Assert.True(getResult.IsSuccess);
        Assert.Equal(29, getResult.Value!.Age);
    }

    [Fact]
    public async Task ListAsync_ShouldMapInvalidPaginationToValidationError()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        var candidateRepository = new CandidateProfileRepository(context);
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

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
        var fieldDefinitionRepository = new DefaultFieldsRepository(context);

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
