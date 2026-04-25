using Microsoft.AspNetCore.Mvc;
using Kesa.Services;

namespace Kesa.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiV1ControllerBase : ControllerBase
{
    /// <summary>
    /// Maps a failed non-generic service result into a standardized HTTP response.
    /// </summary>
    /// <param name="result">Service result to map.</param>
    /// <returns>Action result representing the mapped failure.</returns>
    protected ActionResult MapFailure(ServiceResult result)
    {
        return MapServiceError(result.Error);
    }

    /// <summary>
    /// Maps a failed generic service result into a standardized HTTP response.
    /// </summary>
    /// <typeparam name="T">Result payload type.</typeparam>
    /// <param name="result">Service result to map.</param>
    /// <returns>Action result representing the mapped failure.</returns>
    protected ActionResult MapFailure<T>(ServiceResult<T> result)
    {
        return MapServiceError(result.Error);
    }

    private ActionResult MapServiceError(ServiceError? error)
    {
        var mappedError = error ?? new ServiceError(ServiceErrorCodes.Unexpected, "Unexpected service error.");

        if (mappedError.Code == ServiceErrorCodes.ValidationError)
        {
            var validationErrors = mappedError.ValidationErrors ?? new Dictionary<string, string[]>
            {
                ["request"] = [mappedError.Message]
            };

            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = mappedError.Message,
                Type = "https://httpstatuses.com/400"
            };
            validationProblem.Extensions["errorCode"] = mappedError.Code;
            validationProblem.Extensions["errors"] = validationErrors;

            return BadRequest(validationProblem);
        }

        var statusCode = mappedError.Code switch
        {
            ServiceErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ServiceErrorCodes.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status404NotFound => "Resource not found",
                StatusCodes.Status409Conflict => "Request conflict",
                _ => "Service operation failed"
            },
            Detail = mappedError.Message,
            Type = $"https://httpstatuses.com/{statusCode}"
        };
        problem.Extensions["errorCode"] = mappedError.Code;

        return StatusCode(statusCode, problem);
    }
}
