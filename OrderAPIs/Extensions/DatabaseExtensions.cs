using Microsoft.EntityFrameworkCore;

namespace OrderAPIs.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<OrderDbContext>();
            await context.Database.MigrateAsync();
            Console.WriteLine(">>> [DATABASE ENGINE] Migrations applied successfully inside Docker!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> [DATABASE ENGINE] Error creating database: {ex.Message}");
        }
    }
}
