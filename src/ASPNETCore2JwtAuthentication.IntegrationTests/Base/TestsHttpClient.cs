using ASPNETCore2JwtAuthentication.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.IntegrationTests.Base;

internal static class TestsHttpClient
{
    private static readonly CustomWebApplicationFactory Factory = new();

    private static readonly Lazy<TestsHttpClientModel> ServiceProviderBuilder =
        new(GetClientModel, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     A lazy loaded thread-safe singleton
    /// </summary>
    public static TestsHttpClientModel Instance { get; } = ServiceProviderBuilder.Value;

    private static TestsHttpClientModel GetClientModel()
    {
        var httpClient = Factory.CreateClient();
        var services = Factory.Services;
        var settings = services.GetRequiredService<IOptions<AdminUserSeed>>().Value;
        var generator = services.GetRequiredService<LinkGenerator>();

        return new TestsHttpClientModel
               {
                   HttpClient = httpClient,
                   ServiceProvider = services,
                   AdminUserSeed = settings,
                   LinkGenerator = generator,
               }; //NOTE: This action is very time consuming, so it should be defined as a singleton.
    }
}