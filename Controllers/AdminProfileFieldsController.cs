using Kesa.Controllers.Contracts;
using Kesa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kesa.Controllers;

/// <summary>
/// Exposes admin CRUD endpoints for profile field definitions.
/// </summary>
[Route("api/v1/admin/profile-fields")]
public sealed class AdminProfileFieldsController(IProfileFieldDefinitionService service) : ApiV1ControllerBase
{
    /// <summary>
    /// Lists all profile field definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of field definitions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProfileFieldDefinitionApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<ProfileFieldDefinitionApiResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await service.ListAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        var mapped = result.Value!
            .Select(ToApiResponse)
            .ToArray();

        return Ok(mapped);
    }

    /// <summary>
    /// Gets one profile field definition by id.
    /// </summary>
    /// <param name="id">Field definition id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Field definition data.</returns>
    [HttpGet("{id:guid}", Name = "GetAdminProfileFieldById")]
    [ProducesResponseType(typeof(ProfileFieldDefinitionApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProfileFieldDefinitionApiResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        return Ok(ToApiResponse(result.Value!));
    }

    /// <summary>
    /// Creates a profile field definition.
    /// </summary>
    /// <param name="request">Create request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created field definition.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProfileFieldDefinitionApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProfileFieldDefinitionApiResponse>> CreateAsync(
        [FromBody] CreateProfileFieldDefinitionApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceRequest = new CreateProfileFieldDefinitionRequest
        {
            Name = request.Name,
            Key = request.Key,
            DataType = request.DataType,
            IsRequired = request.IsRequired,
            IsActive = request.IsActive,
            Options = request.Options,
            CreatedBy = request.CreatedBy
        };

        var result = await service.CreateAsync(serviceRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        var response = ToApiResponse(result.Value!);
        return CreatedAtRoute("GetAdminProfileFieldById", new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates a profile field definition.
    /// </summary>
    /// <param name="id">Field definition id.</param>
    /// <param name="request">Update request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated field definition.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProfileFieldDefinitionApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProfileFieldDefinitionApiResponse>> UpdateAsync(
        Guid id,
        [FromBody] UpdateProfileFieldDefinitionApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceRequest = new UpdateProfileFieldDefinitionRequest
        {
            Name = request.Name,
            Key = request.Key,
            DataType = request.DataType,
            IsRequired = request.IsRequired,
            IsActive = request.IsActive,
            Options = request.Options,
            UpdatedBy = request.UpdatedBy
        };

        var result = await service.UpdateAsync(id, serviceRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        return Ok(ToApiResponse(result.Value!));
    }

    /// <summary>
    /// Deletes a profile field definition.
    /// </summary>
    /// <param name="id">Field definition id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content when deletion succeeds.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        return NoContent();
    }

    private static ProfileFieldDefinitionApiResponse ToApiResponse(ProfileFieldDefinitionResponse response)
    {
        return new ProfileFieldDefinitionApiResponse
        {
            Id = response.Id,
            Name = response.Name,
            Key = response.Key,
            DataType = response.DataType,
            IsRequired = response.IsRequired,
            IsActive = response.IsActive,
            Options = response.Options,
            CreatedBy = response.CreatedBy,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };
    }
}
