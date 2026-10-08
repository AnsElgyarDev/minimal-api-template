using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCompany.MinimalApi.Data;

namespace MyCompany.MinimalApi.Tests;

/// <summary>
/// Starts the real API inside the test process, but with a throw-away in-memory SQLite database
/// and fixed test settings, so tests never touch your real database or user-secrets.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    // An in-memory SQLite database lives only while this connection is open.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ApiFactory()
    {
        // Program.cs reads these while the builder is created, so environment variables are the safest way.
        Environment.SetEnvironmentVariable("Jwt__Key", "test-jwt-key-test-jwt-key-test-jwt-key-1234");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("EncryptionSettings__Key", Convert.ToBase64String(new byte[32]));

        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server setup of AppDbContext. EF Core 9+ keeps part of it in an extra
            // "IDbContextOptionsConfiguration<AppDbContext>" registration, so we match that one by name.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().Name.StartsWith("IDbContextOptionsConfiguration") &&
                     d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext)))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Create the tables from the model (no migrations needed in tests).
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}