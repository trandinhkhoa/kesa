using Kesa.Controllers.Contracts;
using Kesa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kesa.Controllers;

/// <summary>
/// Exposes candidate profile CRUD and list endpoints.
/// </summary>
public sealed class CandidatesController(ICandidateProfileService service) : ApiV1ControllerBase
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;

    /// <summary>
    /// Creates a candidate profile.
    /// </summary>
    /// <param name="request">Create request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created candidate profile.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CandidateApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CandidateApiResponse>> CreateAsync(
        [FromBody] CreateCandidateApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceRequest = new CreateCandidateProfileRequest
        {
            Name = request.Name,
            BirthDate = request.BirthDate,
            Sex = request.Sex,
            CustomFields = request.CustomFields,
            CreatedByUserId = request.CreatedByUserId
        };

        var result = await service.CreateAsync(serviceRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        var response = ToApiResponse(result.Value!);
        return CreatedAtRoute("GetCandidateById", new { id = response.Id }, response);
    }

    /// <summary>
    /// Gets one candidate profile by id.
    /// </summary>
    /// <param name="id">Candidate id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Candidate profile response.</returns>
    [HttpGet("{id:guid}", Name = "GetCandidateById")]
    [ProducesResponseType(typeof(CandidateApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CandidateApiResponse>> GetByIdAsync(
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
    /// Lists candidate profiles with default and bounded pagination.
    /// </summary>
    /// <param name="pageNumber">Optional 1-based page number, defaults to 1.</param>
    /// <param name="pageSize">Optional page size, defaults to 20 and clamps to 100 max.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated candidate response.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CandidateListApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CandidateListApiResponse>> ListAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = pageNumber.GetValueOrDefault(DefaultPageNumber);
        var normalizedPageSize = pageSize.GetValueOrDefault(DefaultPageSize);
        if (normalizedPageSize > MaximumPageSize)
        {
            normalizedPageSize = MaximumPageSize;
        }

        var result = await service.ListAsync(normalizedPageNumber, normalizedPageSize, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        return Ok(ToApiListResponse(result.Value!));
    }

    /// <summary>
    /// Updates a candidate profile.
    /// </summary>
    /// <param name="id">Candidate id.</param>
    /// <param name="request">Update request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated candidate profile.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CandidateApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CandidateApiResponse>> UpdateAsync(
        Guid id,
        [FromBody] UpdateCandidateApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceRequest = new UpdateCandidateProfileRequest
        {
            Name = request.Name,
            BirthDate = request.BirthDate,
            Sex = request.Sex,
            CustomFields = request.CustomFields,
            UpdatedByUserId = request.UpdatedByUserId
        };

        var result = await service.UpdateAsync(id, serviceRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapFailure(result);
        }

        return Ok(ToApiResponse(result.Value!));
    }

    /// <summary>
    /// Deletes a candidate profile.
    /// </summary>
    /// <param name="id">Candidate id.</param>
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

    private static CandidateApiResponse ToApiResponse(CandidateProfileResponse response)
    {
        return new CandidateApiResponse
        {
            Id = response.Id,
            Name = response.Name,
            BirthDate = response.BirthDate,
            Age = response.Age,
            Sex = response.Sex,
            CustomFields = response.CustomFields,
            CreatedByUserId = response.CreatedByUserId,
            UpdatedByUserId = response.UpdatedByUserId,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };
    }

    private static CandidateListApiResponse ToApiListResponse(CandidateProfileListResponse response)
    {
        return new CandidateListApiResponse
        {
            Items = response.Items.Select(ToApiResponse).ToArray(),
            TotalCount = response.TotalCount,
            PageNumber = response.PageNumber,
            PageSize = response.PageSize
        };
    }
}
