using System;
using System.Linq;
using ASPNETCore2JwtAuthentication.Common;
using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ASPNETCore2JwtAuthentication.Services
{
    public interface IDbInitializerService
    {
        /// <summary>
        /// Applies any pending migrations for the context to the database.
        /// Will create the database if it does not already exist.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds some default values to the Db
        /// </summary>
        void SeedData();
    }

    public class DbInitializerService : IDbInitializerService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISecurityService _securityService;

        public DbInitializerService(
            IServiceScopeFactory scopeFactory,
            ISecurityService securityService)
        {
            _scopeFactory = scopeFactory;
            _scopeFactory.CheckArgumentIsNull(nameof(_scopeFactory));

            _securityService = securityService;
            _securityService.CheckArgumentIsNull(nameof(_securityService));
        }

        public void Initialize()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.Migrate();
        }

        public void SeedData()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
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
        }
    }
}