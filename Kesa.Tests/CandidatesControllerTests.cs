using System.Text.Json;
using Kesa.Controllers;
using Kesa.Controllers.Contracts;
using Kesa.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// Unit tests for <see cref="CandidatesController"/> request/response behavior.
/// </summary>
public sealed class CandidatesControllerTests
{
    [Fact]
    public async Task ListAsync_ShouldApplyDefaultPaginationAndReturnOk()
    {
        var service = new FakeCandidateProfileService
        {
            ListHandler = (pageNumber, pageSize, _) =>
            {
                Assert.Equal(1, pageNumber);
                Assert.Equal(20, pageSize);

                var payload = new CandidateProfileListResponse
                {
                    Items = [],
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Task.FromResult(ServiceResult<CandidateProfileListResponse>.Success(payload));
            }
        };

        var controller = new CandidatesController(service);

        var result = await controller.ListAsync(null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task ListAsync_ShouldClampPageSizeAtMaximum()
    {
        var service = new FakeCandidateProfileService
        {
            ListHandler = (_, pageSize, _) =>
            {
                Assert.Equal(100, pageSize);

                return Task.FromResult(ServiceResult<CandidateProfileListResponse>.Success(new CandidateProfileListResponse
                {
                    Items = [],
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = pageSize
                }));
            }
        };

        var controller = new CandidatesController(service);

        var result = await controller.ListAsync(1, 999);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnValidationProblemWhenServiceFailsValidation()
    {
        var service = new FakeCandidateProfileService
        {
            CreateHandler = (_, _) => Task.FromResult(ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(
                ServiceErrorCodes.ValidationError,
                "Validation failed.",
                new Dictionary<string, string[]> { ["name"] = ["Name is required."] })))
        };

        var controller = new CandidatesController(service);

        var request = new CreateCandidateApiRequest
        {
            Name = string.Empty,
            BirthDate = new DateOnly(2000, 1, 1),
            Sex = "Other",
            CustomFields = new Dictionary<string, JsonElement>()
        };

        var result = await controller.CreateAsync(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    private sealed class FakeCandidateProfileService : ICandidateProfileService
    {
        public Func<CreateCandidateProfileRequest, CancellationToken, Task<ServiceResult<CandidateProfileResponse>>>? CreateHandler { get; init; }

        public Func<int, int, CancellationToken, Task<ServiceResult<CandidateProfileListResponse>>>? ListHandler { get; init; }

        public Task<ServiceResult<CandidateProfileResponse>> CreateAsync(CreateCandidateProfileRequest request, CancellationToken cancellationToken = default)
            => CreateHandler?.Invoke(request, cancellationToken)
               ?? Task.FromResult(ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(ServiceErrorCodes.Unexpected, "Not configured")));

        public Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(ServiceResult.Success());

        public Task<ServiceResult<CandidateProfileResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(ServiceErrorCodes.Unexpected, "Not configured")));

        public Task<ServiceResult<CandidateProfileListResponse>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
            => ListHandler?.Invoke(pageNumber, pageSize, cancellationToken)
               ?? Task.FromResult(ServiceResult<CandidateProfileListResponse>.Failure(new ServiceError(ServiceErrorCodes.Unexpected, "Not configured")));

        public Task<ServiceResult<CandidateProfileResponse>> UpdateAsync(Guid id, UpdateCandidateProfileRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(ServiceResult<CandidateProfileResponse>.Failure(new ServiceError(ServiceErrorCodes.Unexpected, "Not configured")));
    }
}
