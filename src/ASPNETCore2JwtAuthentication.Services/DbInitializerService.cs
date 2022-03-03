using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ASPNETCore2JwtAuthentication.Services;

public class DbInitializerService : IDbInitializerService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISecurityService _securityService;

    public DbInitializerService(
        IServiceScopeFactory scopeFactory,
        ISecurityService securityService)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
    }

    public void Initialize()
    {
        using var serviceScope = _scopeFactory.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    public void SeedData()
    {
        using var serviceScope = _scopeFactory.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Add default roles
        var adminRole = new Role { Name = CustomRoles.Admin };
        var userRole = new Role { Name = CustomRoles.User };
        if (!context.Roles.Any())
        {
            context.Add(adminRole);
            context.Add(userRole);
            context.SaveChanges();
        }

        // Add Admin user
        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                Username = "Vahid",
                DisplayName = "وحيد",
                IsActive = true,
                LastLoggedIn = null,
                Password = _securityService.GetSha256Hash("1234"),
                SerialNumber = Guid.NewGuid().ToString("N")
            };
            context.Add(adminUser);
            context.SaveChanges();

            context.Add(new UserRole { Role = adminRole, User = adminUser });
            context.Add(new UserRole { Role = userRole, User = adminUser });
            context.SaveChanges();
        }
    }
}