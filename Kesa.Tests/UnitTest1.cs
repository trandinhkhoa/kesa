using Testcontainers.PostgreSql;

namespace Kesa.Tests;

public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("kesa_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public bool DockerUnavailable { get; private set; }

    public string? DockerUnavailableReason { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();
        }
        catch (Exception ex)
        {
            DockerUnavailable = true;
            DockerUnavailableReason = ex.Message;
        }
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
    public const string Name = "postgresql-container";
}

[Collection(PostgreSqlCollection.Name)]
public class PostgreSqlInfrastructureTests(PostgreSqlContainerFixture fixture)
{
    [Fact]
    public void ShouldProvideConnectionStringFromContainer()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");

        Assert.False(string.IsNullOrWhiteSpace(fixture.ConnectionString));
    }
}
