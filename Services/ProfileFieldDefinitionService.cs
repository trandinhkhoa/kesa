using System.Text.Json;
using Kesa.Models;
using Kesa.Repositories;

namespace Kesa.Services;

/// <summary>
/// Implements business rules for profile field definition operations.
/// </summary>
public sealed class ProfileFieldDefinitionService(
    IProfileFieldDefinitionRepository repository,
    ILogger<ProfileFieldDefinitionService> logger) : IProfileFieldDefinitionService
{
    private static readonly HashSet<string> SupportedDataTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "String",
        "Number",
        "Date",
        "Boolean",
        "Enum"
    };

    /// <inheritdoc />
    public async Task<ServiceResult<ProfileFieldDefinitionResponse>> CreateAsync(
        CreateProfileFieldDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateRequest(request.Name, request.Key, request.DataType, request.IsRequired, request.IsActive, request.Options);
        if (validationResult is not null)
        {
            logger.LogWarning("Profile field definition create validation failed for key {FieldKey}", request.Key);
            return ServiceResult<ProfileFieldDefinitionResponse>.Failure(validationResult);
        }

        try
        {
            var existingByKey = await repository.GetByKeyAsync(request.Key.Trim(), cancellationToken);
            if (existingByKey is not null)
            {
                logger.LogWarning("Profile field definition create conflict for existing key {FieldKey}", request.Key);
                return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.Conflict,
                    $"A field definition with key '{request.Key}' already exists."));
            }

            var now = DateTime.UtcNow;
            var entity = new ProfileFieldDefinition
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Key = request.Key.Trim(),
                DataType = NormalizeDataType(request.DataType),
                IsRequired = request.IsRequired,
                IsActive = request.IsActive,
                OptionsJson = ToOptionsJson(request.DataType, request.Options),
                CreatedBy = request.CreatedBy,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await repository.CreateAsync(entity, cancellationToken);
            logger.LogInformation("Profile field definition created with id {FieldDefinitionId}", created.Id);

            return ServiceResult<ProfileFieldDefinitionResponse>.Success(ToResponse(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating profile field definition for key {FieldKey}", request.Key);
            return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while creating profile field definition."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProfileFieldDefinitionResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await repository.GetByIdAsync(id, cancellationToken);
            if (entity is null)
            {
                logger.LogWarning("Profile field definition not found for id {FieldDefinitionId}", id);
                return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Profile field definition '{id}' was not found."));
            }

            return ServiceResult<ProfileFieldDefinitionResponse>.Success(ToResponse(entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving profile field definition {FieldDefinitionId}", id);
            return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while retrieving profile field definition."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProfileFieldDefinitionResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await repository.ListAsync(cancellationToken);
            var items = entities.Select(ToResponse).ToArray();
            return ServiceResult<IReadOnlyList<ProfileFieldDefinitionResponse>>.Success(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error listing profile field definitions");
            return ServiceResult<IReadOnlyList<ProfileFieldDefinitionResponse>>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while listing profile field definitions."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProfileFieldDefinitionResponse>> UpdateAsync(
        Guid id,
        UpdateProfileFieldDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateRequest(request.Name, request.Key, request.DataType, request.IsRequired, request.IsActive, request.Options);
        if (validationResult is not null)
        {
            logger.LogWarning("Profile field definition update validation failed for id {FieldDefinitionId}", id);
            return ServiceResult<ProfileFieldDefinitionResponse>.Failure(validationResult);
        }

        try
        {
            var existing = await repository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
            {
                logger.LogWarning("Profile field definition update target not found for id {FieldDefinitionId}", id);
                return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Profile field definition '{id}' was not found."));
            }

            var existingByKey = await repository.GetByKeyAsync(request.Key.Trim(), cancellationToken);
            if (existingByKey is not null && existingByKey.Id != id)
            {
                logger.LogWarning("Profile field definition update key conflict for key {FieldKey}", request.Key);
                return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.Conflict,
                    $"A field definition with key '{request.Key}' already exists."));
            }

            existing.Name = request.Name.Trim();
            existing.Key = request.Key.Trim();
            existing.DataType = NormalizeDataType(request.DataType);
            existing.IsRequired = request.IsRequired;
            existing.IsActive = request.IsActive;
            existing.OptionsJson = ToOptionsJson(request.DataType, request.Options);
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await repository.UpdateAsync(existing, cancellationToken);
            if (updated is null)
            {
                logger.LogWarning("Profile field definition update failed because entity disappeared for id {FieldDefinitionId}", id);
                return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Profile field definition '{id}' was not found."));
            }

            logger.LogInformation("Profile field definition updated for id {FieldDefinitionId}", id);
            return ServiceResult<ProfileFieldDefinitionResponse>.Success(ToResponse(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating profile field definition {FieldDefinitionId}", id);
            return ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while updating profile field definition."));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await repository.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                logger.LogWarning("Profile field definition delete target not found for id {FieldDefinitionId}", id);
                return ServiceResult.Failure(new ServiceError(
                    ServiceErrorCodes.NotFound,
                    $"Profile field definition '{id}' was not found."));
            }

            logger.LogInformation("Profile field definition deleted for id {FieldDefinitionId}", id);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting profile field definition {FieldDefinitionId}", id);
            return ServiceResult.Failure(new ServiceError(
                ServiceErrorCodes.Unexpected,
                "Unexpected error occurred while deleting profile field definition."));
        }
    }

    private static ServiceError? ValidateRequest(
        string name,
        string key,
        string dataType,
        bool isRequired,
        bool isActive,
        IReadOnlyList<string>? options)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Name is required."];
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            errors["key"] = ["Key is required."];
        }

        if (string.IsNullOrWhiteSpace(dataType))
        {
            errors["dataType"] = ["DataType is required."];
        }
        else if (!SupportedDataTypes.Contains(dataType.Trim()))
        {
            errors["dataType"] = [$"DataType '{dataType}' is not supported."];
        }

        if (isRequired && !isActive)
        {
            errors["isRequired"] = ["A required field must be active."];
        }

        if (dataType.Equals("Enum", StringComparison.OrdinalIgnoreCase))
        {
            if (options is null || options.Count == 0)
            {
                errors["options"] = ["Enum fields must provide at least one option."];
            }
            else if (options.Any(string.IsNullOrWhiteSpace))
            {
                errors["options"] = ["Enum options cannot be blank."];
            }
        }

        return errors.Count == 0
            ? null
            : new ServiceError(ServiceErrorCodes.ValidationError, "Validation failed.", errors);
    }

    private static string NormalizeDataType(string dataType)
    {
        return SupportedDataTypes.First(x => x.Equals(dataType.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string? ToOptionsJson(string dataType, IReadOnlyList<string>? options)
    {
        if (!dataType.Equals("Enum", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalizedOptions = options!
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return JsonSerializer.Serialize(normalizedOptions);
    }

    private static ProfileFieldDefinitionResponse ToResponse(ProfileFieldDefinition entity)
    {
        return new ProfileFieldDefinitionResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Key = entity.Key,
            DataType = entity.DataType,
            IsRequired = entity.IsRequired,
            IsActive = entity.IsActive,
            Options = ParseOptions(entity.OptionsJson),
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static IReadOnlyList<string>? ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(optionsJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
