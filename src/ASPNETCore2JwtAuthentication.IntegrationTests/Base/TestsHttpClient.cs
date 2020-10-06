using System;
using System.Threading;
using System.Net.Http;

namespace ASPNETCore2JwtAuthentication.IntegrationTests
{
    public static class TestsHttpClient
    {
        private static readonly Lazy<HttpClient> _serviceProviderBuilder =
                new Lazy<HttpClient>(getHttpClient, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// A lazy loaded thread-safe singleton
        /// </summary>
        public static HttpClient Instance { get; } = _serviceProviderBuilder.Value;

        private static HttpClient getHttpClient()
        {
            var services = new CustomWebApplicationFactory();
            return services.CreateClient(); //NOTE: This action is very time consuming, so it should be defined as a singleton.
        }
    }
}