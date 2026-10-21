using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HireFlow.Infrastructure.Data;

// Runs once at startup: creates or migrates the database, then adds demo data if it is empty.
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // SQL Server in Docker can take a few seconds to start, so we retry a few times.
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await CreateDatabaseAsync(db, config["Database:Provider"]);
                await DatabaseSeeder.SeedAsync(db);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning("Database not ready (attempt {Attempt}/{Max}): {Message}", attempt, maxAttempts, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    private static async Task CreateDatabaseAsync(AppDbContext db, string? provider)
    {
        // If you created migrations (dotnet ef migrations add InitialCreate), SQL Server uses them.
        // Otherwise (and always for SQLite) the tables are created directly from the model.
        var hasMigrations = db.Database.GetMigrations().Any();
        if (provider != "Sqlite" && hasMigrations)
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }
    }
}
