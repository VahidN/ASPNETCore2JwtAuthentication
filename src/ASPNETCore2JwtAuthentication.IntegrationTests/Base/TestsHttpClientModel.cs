using ASPNETCore2JwtAuthentication.Models;
using Microsoft.AspNetCore.Routing;

namespace ASPNETCore2JwtAuthentication.IntegrationTests.Base;

public class TestsHttpClientModel
{
    public HttpClient HttpClient { set; get; } = default!;

    public IServiceProvider ServiceProvider { set; get; } = default!;

    public AdminUserSeed AdminUserSeed { set; get; } = default!;

    public LinkGenerator LinkGenerator { set; get; } = default!;
}