using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ASPNETCore2JwtAuthentication.ConsoleClient
{
    //Note: First you should run the `ASPNETCore2JwtAuthentication.WebApp` project and then run the `ConsoleClient` project.

    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    class Program
    {
        private static readonly string _baseAddress = "https://localhost:5001/";
        private static readonly HttpClientHandler _httpClientHandler = new HttpClientHandler
        {
            UseCookies = true,
            UseDefaultCredentials = true,
            CookieContainer = new CookieContainer()
        };
        private static readonly HttpClient _httpClient = new HttpClient(_httpClientHandler)
        {
            BaseAddress = new Uri(_baseAddress)
        };

        public static async Task Main(string[] args)
        {
            var (token, appCookies) = await LoginAsync(
                requestUri: "/api/account/login",
                username: "Vahid",
                password: "1234");
            await CallProtectedApiAsync(requestUri: "/api/MyProtectedApi", token: token);
            var newToken = await RefreshTokenAsync("/api/account/RefreshToken", token, appCookies);
        }

        private static Dictionary<string, string> GetAntiforgeryCookies()
        {
            Console.WriteLine("\nGet Antiforgery Cookies:");
            var cookies = _httpClientHandler.CookieContainer.GetCookies(new Uri(_baseAddress));

            var appCookies = new Dictionary<string, string>();
            Console.WriteLine("WebApp Cookies:");
            foreach (Cookie cookie in cookies)
            {
                Console.WriteLine($"Name : {cookie.Name}");
                Console.WriteLine($"Value: {cookie.Value}");
                appCookies.Add(cookie.Name, cookie.Value);
            }
            return appCookies;
        }

        private static async Task<(Token Token, Dictionary<string, string> AppCookies)> LoginAsync(string requestUri, string username, string password)
        {
            Console.WriteLine("\nLogin:");

            var response = await _httpClient.PostAsJsonAsync(
                 requestUri,
                 new User { Username = username, Password = password });
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() });
            Console.WriteLine($"Response    : {response}");
            Console.WriteLine($"AccessToken : {token.AccessToken}");
            Console.WriteLine($"RefreshToken: {token.RefreshToken}");

            var appCookies = GetAntiforgeryCookies();
            return (token, appCookies);
        }

        private static async Task<Token> RefreshTokenAsync(string requestUri, Token token, Dictionary<string, string> appCookies)
        {
            Console.WriteLine("\nRefreshToken:");

            if (!_httpClient.DefaultRequestHeaders.Contains("X-XSRF-TOKEN"))
            {
                // this is necessary for [AutoValidateAntiforgeryTokenAttribute] and all of the 'POST' requests
                _httpClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", appCookies["XSRF-TOKEN"]);
            }
            // this is necessary to populate the this.HttpContext.User object automatically
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _httpClient.PostAsJsonAsync(
                 requestUri,
                 new { refreshToken = token.RefreshToken });
            response.EnsureSuccessStatusCode();

            var newToken = await response.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() });
            Console.WriteLine($"Response    : {response}");
            Console.WriteLine($"New AccessToken : {newToken.AccessToken}");
            Console.WriteLine($"New RefreshToken: {newToken.RefreshToken}");
            return newToken;
        }

        private static async Task CallProtectedApiAsync(string requestUri, Token token)
        {
            Console.WriteLine("\nCall ProtectedApi:");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _httpClient.GetAsync(requestUri);
            var message = await response.Content.ReadAsStringAsync();
            Console.WriteLine("URL response: " + message);
        }
    }
}
