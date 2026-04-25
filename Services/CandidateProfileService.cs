using System.Text.Json;
using Kesa.Models;
using Kesa.Repositories;

namespace Kesa.Services;

/// <summary>
/// Implements business rules for candidate profile operations.
/// </summary>
public sealed class CandidateProfileService(
    ICandidateProfileRepository candidateRepository,
    IProfileFieldDefinitionRepository fieldDefinitionRepository,
    ILogger<CandidateProfileService> logger) : ICandidateProfileService
{
    private static readonly HashSet<string> AllowedSexValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Male",
        "Female",
        "Other"
    };

    /// <inheritdoc />
    public async Task<ServiceResult<CandidateProfileResponse>> CreateAsync(
        CreateCandidateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validationError = await ValidateCandidateRequestAsync(request.Name, request.BirthDate, request.Sex, request.CustomFields, cancellationToken);
            if (validationError is not null)
            {
                logger.LogWarning("Candidate create validation failed for name {CandidateName}", request.Name);
                return ServiceResult<CandidateProfileResponse>.Failure(validationError);
            }

            var now = DateTime.UtcNow;
            var entity = new CandidateProfile
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                BirthDate = request.BirthDate,
                Sex = NormalizeSex(request.Sex),
                CustomFields = BuildCustomFieldsJson(request.CustomFields),
                CreatedByUserId = request.CreatedByUserId,
                UpdatedByUserId = request.CreatedByUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await candidateRepository.CreateAsync(entity, cancellationToken);
            logger.LogInformation("Candidate profile created with id {CandidateId}", created.Id);

            return ServiceResult<CandidateProfileResponse>.Success(ToResponse(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating candidate profile for name {CandidateName}", request.Name);
            return ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while creating candidate profile."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CandidateProfileResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var candidate = await candidateRepository.GetByIdAsync(id, cancellationToken);
            if (candidate is null)
            {
                logger.LogWarning("Candidate profile not found for id {CandidateId}", id);
                return ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Candidate profile '{id}' was not found."));
            }

            return ServiceResult<CandidateProfileResponse>.Success(ToResponse(candidate));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving candidate profile {CandidateId}", id);
            return ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while retrieving candidate profile."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CandidateProfileListResponse>> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (items, totalCount) = await candidateRepository.ListAsync(pageNumber, pageSize, cancellationToken);
            var response = new CandidateProfileListResponse
            {
                Items = items.Select(ToResponse).ToArray(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<CandidateProfileListResponse>.Success(response);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            logger.LogWarning(ex, "Candidate list validation failed for pageNumber {PageNumber} and pageSize {PageSize}", pageNumber, pageSize);
            return ServiceResult<CandidateProfileListResponse>.Failure(new ServiceError(
                ServiceErrorCodes.ValidationError,
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "pagination"] = [ex.Message]
                }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error listing candidate profiles for pageNumber {PageNumber} and pageSize {PageSize}", pageNumber, pageSize);
            return ServiceResult<CandidateProfileListResponse>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while listing candidate profiles."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CandidateProfileResponse>> UpdateAsync(
        Guid id,
        UpdateCandidateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await candidateRepository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
            {
                logger.LogWarning("Candidate profile update target not found for id {CandidateId}", id);
                return ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Candidate profile '{id}' was not found."));
            }

            var validationError = await ValidateCandidateRequestAsync(request.Name, request.BirthDate, request.Sex, request.CustomFields, cancellationToken);
            if (validationError is not null)
            {
                logger.LogWarning("Candidate update validation failed for id {CandidateId}", id);
                return ServiceResult<CandidateProfileResponse>.Failure(validationError);
            }

            existing.Name = request.Name.Trim();
            existing.BirthDate = request.BirthDate;
            existing.Sex = NormalizeSex(request.Sex);
            existing.CustomFields = BuildCustomFieldsJson(request.CustomFields);
            existing.UpdatedByUserId = request.UpdatedByUserId;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await candidateRepository.UpdateAsync(existing, cancellationToken);
            if (updated is null)
            {
                logger.LogWarning("Candidate profile update failed because entity disappeared for id {CandidateId}", id);
                return ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Candidate profile '{id}' was not found."));
            }

            logger.LogInformation("Candidate profile updated for id {CandidateId}", id);
            return ServiceResult<CandidateProfileResponse>.Success(ToResponse(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating candidate profile {CandidateId}", id);
            return ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while updating candidate profile."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await candidateRepository.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                logger.LogWarning("Candidate profile delete target not found for id {CandidateId}", id);
                return ServiceResult.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Candidate profile '{id}' was not found."));
            }

            logger.LogInformation("Candidate profile deleted for id {CandidateId}", id);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting candidate profile {CandidateId}", id);
            return ServiceResult.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while deleting candidate profile."));
        }
    }

    private async Task<ServiceError?> ValidateCandidateRequestAsync(
        string name,
        DateOnly birthDate,
        string sex,
        Dictionary<string, JsonElement> customFields,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(name))
        {
            AddError(errors, "name", "Name is required.");
        }

        if (birthDate == default)
        {
            AddError(errors, "birthDate", "BirthDate is required.");
        }
        else if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            AddError(errors, "birthDate", "BirthDate cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(sex))
        {
            AddError(errors, "sex", "Sex is required.");
        }
        else if (!AllowedSexValues.Contains(sex.Trim()))
        {
            AddError(errors, "sex", "Sex must be one of: Male, Female, Other.");
        }

        var fieldDefinitions = await fieldDefinitionRepository.ListAsync(cancellationToken);
        var activeDefinitions = fieldDefinitions
            .Where(x => x.IsActive)
            .ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        var allDefinitions = fieldDefinitions
            .ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var activeRequiredField in activeDefinitions.Values.Where(x => x.IsRequired))
        {
            if (!customFields.TryGetValue(activeRequiredField.Key, out var requiredValue) || requiredValue.ValueKind == JsonValueKind.Null)
            {
                AddError(errors, $"customFields.{activeRequiredField.Key}", "Required custom field value is missing.");
            }
        }

        foreach (var (key, value) in customFields)
        {
            if (!allDefinitions.TryGetValue(key, out var definition))
            {
                AddError(errors, $"customFields.{key}", "Unknown custom field key.");
                continue;
            }

            if (!definition.IsActive)
            {
                AddError(errors, $"customFields.{key}", "Inactive custom field key is not allowed.");
                continue;
            }

            var typeValidationError = ValidateCustomFieldValueType(definition, value);
            if (typeValidationError is not null)
            {
                AddError(errors, $"customFields.{key}", typeValidationError);
            }
        }

        if (errors.Count == 0)
        {
            return null;
        }

        return new ServiceError(
            ServiceErrorCodes.ValidationError,
            "Validation failed.",
            errors.ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeSex(string sex)
    {
        return AllowedSexValues.First(x => x.Equals(sex.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string? ValidateCustomFieldValueType(ProfileFieldDefinition definition, JsonElement value)
    {
        return definition.DataType switch
        {
            "String" when value.ValueKind != JsonValueKind.String => "Expected a string value.",
            "Number" when value.ValueKind != JsonValueKind.Number => "Expected a numeric value.",
            "Date" when !IsValidDate(value) => "Expected a valid date string in YYYY-MM-DD format.",
            "Boolean" when value.ValueKind is not JsonValueKind.True and not JsonValueKind.False => "Expected a boolean value.",
            "Enum" => ValidateEnumValue(definition, value),
            _ => null
        };
    }

    private static string? ValidateEnumValue(ProfileFieldDefinition definition, JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            return "Expected a string enum value.";
        }

        if (string.IsNullOrWhiteSpace(definition.OptionsJson))
        {
            return "Enum field definition options are missing.";
        }

        try
        {
            var options = JsonSerializer.Deserialize<string[]>(definition.OptionsJson);
            if (options is null || options.Length == 0)
            {
                return "Enum field definition options are missing.";
            }

            var candidateValue = value.GetString();
            if (candidateValue is null || !options.Contains(candidateValue, StringComparer.OrdinalIgnoreCase))
            {
                return "Value is not in allowed enum options.";
            }

            return null;
        }
        catch (JsonException)
        {
            return "Enum field definition options are invalid.";
        }
    }

    private static bool IsValidDate(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var dateString = value.GetString();
        return DateOnly.TryParseExact(dateString, "yyyy-MM-dd", out _);
    }

    private static JsonDocument BuildCustomFieldsJson(Dictionary<string, JsonElement> customFields)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();
            foreach (var (key, value) in customFields.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                writer.WritePropertyName(key);
                value.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        return JsonDocument.Parse(memoryStream.ToArray());
    }

    private static CandidateProfileResponse ToResponse(CandidateProfile entity)
    {
        return new CandidateProfileResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            BirthDate = entity.BirthDate,
            Age = CalculateAge(entity.BirthDate),
            Sex = entity.Sex,
            CustomFields = ParseCustomFields(entity.CustomFields),
            CreatedByUserId = entity.CreatedByUserId,
            UpdatedByUserId = entity.UpdatedByUserId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static Dictionary<string, JsonElement> ParseCustomFields(JsonDocument customFieldsDocument)
    {
        var parsed = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in customFieldsDocument.RootElement.EnumerateObject())
        {
            parsed[property.Name] = property.Value.Clone();
        }

        return parsed;
    }

    private static void AddError(
        IDictionary<string, List<string>> errors,
        string key,
        string message)
    {
        if (!errors.TryGetValue(key, out var values))
        {
            values = [];
            errors[key] = values;
        }

        values.Add(message);
    }
}
