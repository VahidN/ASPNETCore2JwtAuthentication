using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using ASPNETCore2JwtAuthentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.Services;

public class DbInitializerService : IDbInitializerService
{
    private readonly IOptions<AdminUserSeed> _adminUserSeedOption;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISecurityService _securityService;

    public DbInitializerService(
        IServiceScopeFactory scopeFactory,
        ISecurityService securityService,
        IOptions<AdminUserSeed> adminUserSeedOption)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _adminUserSeedOption = adminUserSeedOption ?? throw new ArgumentNullException(nameof(adminUserSeedOption));
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
                                Username = _adminUserSeedOption.Value.Username,
                                DisplayName = _adminUserSeedOption.Value.DisplayName,
                                IsActive = true,
                                LastLoggedIn = null,
                                Password = _securityService.GetSha256Hash(_adminUserSeedOption.Value.Password),
                                SerialNumber = Guid.NewGuid().ToString("N"),
                            };
            context.Add(adminUser);
            context.SaveChanges();

            context.Add(new UserRole { Role = adminRole, User = adminUser });
            context.Add(new UserRole { Role = userRole, User = adminUser });
            context.SaveChanges();
        }
    }
}