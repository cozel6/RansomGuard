using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RansomGuard.API.Data;
using Xunit;

namespace RansomGuard.API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RansomGuardDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Create in-memory SQLite connection (persists for test class lifetime)
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add test DbContext with in-memory database
            services.AddDbContext<RansomGuardDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Reduce logging noise in tests
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Apply migrations once when factory is created
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RansomGuardDbContext>();

        await dbContext.Database.MigrateAsync();

        // Verify database initialization succeeded
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (!canConnect)
        {
            throw new InvalidOperationException("Failed to initialize test database");
        }
    }

    public new async Task DisposeAsync()
    {
        // Clean up in-memory database connection
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
