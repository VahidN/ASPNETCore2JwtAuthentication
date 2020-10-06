using ASPNETCore2JwtAuthentication.WebApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ASPNETCore2JwtAuthentication.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = base.CreateWebHostBuilder();
            builder.ConfigureLogging(logging =>
            {
                //TODO: ...
            });
            return builder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Don't run `IHostedService`s when running as a test
                services.RemoveAll(typeof(IHostedService));
            });
        }
    }
}