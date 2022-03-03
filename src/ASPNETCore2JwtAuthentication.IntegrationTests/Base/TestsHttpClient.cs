namespace ASPNETCore2JwtAuthentication.IntegrationTests.Base;

internal static class TestsHttpClient
{
    private static readonly CustomWebApplicationFactory _factory = new();

    private static readonly Lazy<HttpClient> _serviceProviderBuilder =
        new(getHttpClient, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     A lazy loaded thread-safe singleton
    /// </summary>
    public static HttpClient Instance { get; } = _serviceProviderBuilder.Value;

    private static HttpClient getHttpClient()
    {
        var httpClient = _factory.CreateClient();
        return httpClient; //NOTE: This action is very time consuming, so it should be defined as a singleton.
    }
}