using Kesa.Controllers;
using Kesa.Controllers.Contracts;
using Kesa.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// Unit tests for <see cref="AdminProfileFieldsController"/> request/response behavior.
/// </summary>
public sealed class AdminProfileFieldsControllerTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedAtRouteWhenServiceSucceeds()
    {
        var response = new ProfileFieldDefinitionResponse
        {
            Id = Guid.NewGuid(),
            Name = "Department",
            Key = "department",
            DataType = "String",
            IsRequired = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = new FakeProfileFieldDefinitionService
        {
            CreateHandler = (_, _) => Task.FromResult(ServiceResult<ProfileFieldDefinitionResponse>.Success(response))
        };

        var controller = new AdminProfileFieldsController(service);

        var result = await controller.CreateAsync(new CreateProfileFieldDefinitionApiRequest
        {
            Name = "Department",
            Key = "department",
            DataType = "String",
            IsRequired = false,
            IsActive = true
        });

        var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal("GetAdminProfileFieldById", created.RouteName);
        Assert.Equal(201, created.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFoundProblemWhenServiceReturnsNotFound()
    {
        var service = new FakeProfileFieldDefinitionService
        {
            GetByIdHandler = (_, _) => Task.FromResult(ServiceResult<ProfileFieldDefinitionResponse>.Failure(
                new ServiceError(ServiceErrorCodes.NotFound, "Missing")))
        };

        var controller = new AdminProfileFieldsController(service);

        var result = await controller.GetByIdAsync(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    private sealed class FakeProfileFieldDefinitionService : IProfileFieldDefinitionService
    {
        public Func<CreateProfileFieldDefinitionRequest, CancellationToken, Task<ServiceResult<ProfileFieldDefinitionResponse>>>? CreateHandler { get; init; }

        public Func<Guid, CancellationToken, Task<ServiceResult<ProfileFieldDefinitionResponse>>>? GetByIdHandler { get; init; }

        public Task<ServiceResult<ProfileFieldDefinitionResponse>> CreateAsync(CreateProfileFieldDefinitionRequest request, CancellationToken cancellationToken = default)
            => CreateHandler?.Invoke(request, cancellationToken)
               ?? Task.FromResult(ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(ServiceErrorCodes.Unexpected, "Not configured")));

        public Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(ServiceResult.Success());

        public Task<ServiceResult<ProfileFieldDefinitionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdHandler?.Invoke(id, cancellationToken)
               ?? Task.FromResult(ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(ServiceErrorCodes.Unexpected, "Not configured")));

        public Task<ServiceResult<IReadOnlyList<ProfileFieldDefinitionResponse>>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ServiceResult<IReadOnlyList<ProfileFieldDefinitionResponse>>.Success([]));

        public Task<ServiceResult<ProfileFieldDefinitionResponse>> UpdateAsync(Guid id, UpdateProfileFieldDefinitionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(ServiceResult<ProfileFieldDefinitionResponse>.Failure(new ServiceError(ServiceErrorCodes.Unexpected, "Not configured")));
    }
}
