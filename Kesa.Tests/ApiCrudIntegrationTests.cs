using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kesa.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Kesa.Tests;

/// <summary>
/// End-to-end API integration tests covering admin and candidate CRUD workflows.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class ApiCrudIntegrationTests(PostgreSqlContainerFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task AdminProfileFieldsCrudWorkflow_ShouldSucceed()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/v1/admin/profile-fields", new
        {
            name = "SkillLevel",
            key = "skillLevel",
            dataType = "Enum",
            isRequired = false,
            isActive = true,
            options = new[] { "junior", "mid", "senior" }
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await DeserializeAsync<ProfileFieldDefinitionEnvelope>(createResponse);
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/v1/admin/profile-fields/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/admin/profile-fields/{created.Id}", new
        {
            name = "SkillLevelUpdated",
            key = "skillLevel",
            dataType = "Enum",
            isRequired = false,
            isActive = true,
            options = new[] { "junior", "mid", "senior", "staff" }
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/v1/admin/profile-fields/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await client.GetAsync($"/api/v1/admin/profile-fields/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task CandidateAndAdminWorkflow_ShouldSucceedEndToEnd()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");
        await fixture.ResetDatabaseAsync();

        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var fieldResponse = await client.PostAsJsonAsync("/api/v1/admin/profile-fields", new
        {
            name = "Nationality",
            key = "nationality",
            dataType = "String",
            isRequired = true,
            isActive = true
        });
        Assert.Equal(HttpStatusCode.Created, fieldResponse.StatusCode);

        var createCandidateResponse = await client.PostAsJsonAsync("/api/v1/candidates", new
        {
            name = "E2E Candidate",
            birthDate = "1996-03-12",
            sex = "Female",
            customFields = new
            {
                nationality = "Vietnamese"
            }
        });

        Assert.Equal(HttpStatusCode.Created, createCandidateResponse.StatusCode);
        var createdCandidate = await DeserializeAsync<CandidateEnvelope>(createCandidateResponse);
        Assert.NotNull(createdCandidate);

        var listResponse = await client.GetAsync("/api/v1/candidates");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await DeserializeAsync<CandidateListEnvelope>(listResponse);
        Assert.NotNull(listBody);
        Assert.Equal(1, listBody!.TotalCount);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/candidates/{createdCandidate!.Id}", new
        {
            name = "E2E Candidate Updated",
            birthDate = "1996-03-12",
            sex = "Female",
            customFields = new
            {
                nationality = "Vietnamese"
            }
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/v1/candidates/{createdCandidate.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await client.GetAsync($"/api/v1/candidates/{createdCandidate.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<KesaDbContext>>();
                    services.RemoveAll<KesaDbContext>();

                    services.AddDbContext<KesaDbContext>(options =>
                    {
                        options.UseNpgsql(fixture.ConnectionString);
                    });

                    using var scope = services.BuildServiceProvider().CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<KesaDbContext>();
                    db.Database.Migrate();
                });
            });
    }

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private sealed class ProfileFieldDefinitionEnvelope
    {
        public Guid Id { get; set; }
    }

    private sealed class CandidateEnvelope
    {
        public Guid Id { get; set; }
    }

    private sealed class CandidateListEnvelope
    {
        public int TotalCount { get; set; }
    }
}
