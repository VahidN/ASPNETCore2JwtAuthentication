using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ASPNETCore2JwtAuthentication.IntegrationTests.Base;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // How to override settings ...
        builder.ConfigureHostConfiguration(config =>
                                           {
                                               config.AddInMemoryCollection(new Dictionary<string, string?>
                                                                            {
                                                                                {
                                                                                    "ConnectionStrings:SqlServer:ApplicationDbContextConnection",
                                                                                    "newConnectionString ...."
                                                                                },
                                                                            });

                                               // config.AddJsonFile("integrationsettings.json").Build();
                                           });
        var host = base.CreateHost(builder);

        // How to run a command during startup
        // host.InitializeDb()

        return host;
    }

    protected override IWebHostBuilder? CreateWebHostBuilder()
    {
        var builder = base.CreateWebHostBuilder();
        builder?.ConfigureLogging(logging =>
                                  {
                                      //TODO: ...
                                  });
        return builder;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
                                      {
                                          // How to remove or add services
                                          services.RemoveAll(typeof(IHostedService));
                                      });
    }
}