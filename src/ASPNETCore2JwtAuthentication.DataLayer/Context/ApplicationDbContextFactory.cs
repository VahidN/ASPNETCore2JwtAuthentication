using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ASPNETCore2JwtAuthentication.DataLayer.Context;

/// <summary>
///     Only used by EF Tooling
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        WriteLine($"Using `{basePath}` as the BasePath");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            .Replace("|DataDirectory|", Path.Combine(basePath, "wwwroot", "app_data"),
                StringComparison.OrdinalIgnoreCase);
        builder.UseSqlServer(connectionString);
        return new ApplicationDbContext(builder.Options);
    }
}