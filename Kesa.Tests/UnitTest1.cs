using Kesa.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

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

            await using var context = CreateDbContext();
            await context.Database.MigrateAsync();
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

    /// <summary>
    /// Creates a new DbContext instance connected to the test PostgreSQL container.
    /// </summary>
    /// <returns>A configured <see cref="KesaDbContext"/> instance.</returns>
    public KesaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<KesaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new KesaDbContext(options);
    }

    /// <summary>
    /// Resets application tables to ensure test isolation while preserving migration history.
    /// </summary>
    /// <returns>A task that completes when table cleanup is done.</returns>
    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync("DELETE FROM candidate_profiles;");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM profile_field_definitions;");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM users;");
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
    public async Task ShouldProvideConnectionStringAndApplyMigrationsAsync()
    {
        Assert.False(fixture.DockerUnavailable, $"Docker unavailable for testcontainers: {fixture.DockerUnavailableReason}");

        Assert.False(string.IsNullOrWhiteSpace(fixture.ConnectionString));

        await using var context = fixture.CreateDbContext();
        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}
