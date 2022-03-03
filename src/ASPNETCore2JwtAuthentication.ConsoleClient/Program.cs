using System.Net.Http.Formatting;

namespace ASPNETCore2JwtAuthentication.ConsoleClient;
//Note: First you should run the `ASPNETCore2JwtAuthentication.WebApp` project and then run the `ConsoleClient` project.

public static class Program
{
    private const string BaseAddress = "https://localhost:5001/";

    private static readonly HttpClientHandler _httpClientHandler = new()
    {
        UseCookies = true,
        UseDefaultCredentials = true,
        CookieContainer = new CookieContainer()
    };

    private static readonly HttpClient _httpClient = new(_httpClientHandler)
    {
        BaseAddress = new Uri(BaseAddress)
    };

    public static async Task Main(string[] args)
    {
        var (token, appCookies) = await LoginAsync(
            "/api/account/login",
            "Vahid",
            "1234");
        await CallProtectedApiAsync("/api/MyProtectedApi", token);
        _ = await RefreshTokenAsync("/api/account/RefreshToken", token, appCookies);
    }

    private static Dictionary<string, string> GetAntiforgeryCookies()
    {
        WriteLine("\nGet Antiforgery Cookies:");
        var cookies = _httpClientHandler.CookieContainer.GetCookies(new Uri(BaseAddress));

        var appCookies = new Dictionary<string, string>();
        WriteLine("WebApp Cookies:");
        foreach (Cookie cookie in cookies)
        {
            WriteLine($"Name : {cookie.Name}");
            WriteLine($"Value: {cookie.Value}");
            appCookies.Add(cookie.Name, cookie.Value);
        }

        return appCookies;
    }

    private static async Task<(Token Token, Dictionary<string, string> AppCookies)> LoginAsync(
        string requestUri,
        string username,
        string password)
    {
        WriteLine("\nLogin:");

        var response =
            await _httpClient.PostAsJsonAsync(requestUri, new User { Username = username, Password = password });
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() });
        WriteLine($"Response    : {response}");
        WriteLine($"AccessToken : {token.AccessToken}");
        WriteLine($"RefreshToken: {token.RefreshToken}");

        var appCookies = GetAntiforgeryCookies();
        return (token, appCookies);
    }

    private static async Task<Token> RefreshTokenAsync(string requestUri, Token token,
        Dictionary<string, string> appCookies)
    {
        WriteLine("\nRefreshToken:");

        if (!_httpClient.DefaultRequestHeaders.Contains("X-XSRF-TOKEN"))
        {
            // this is necessary for [AutoValidateAntiforgeryTokenAttribute] and all of the 'POST' requests
            _httpClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", appCookies["XSRF-TOKEN"]);
        }

        // this is necessary to populate the this.HttpContext.User object automatically
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var response = await _httpClient.PostAsJsonAsync(requestUri, new { refreshToken = token.RefreshToken });
        response.EnsureSuccessStatusCode();

        var newToken = await response.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() });
        WriteLine($"Response    : {response}");
        WriteLine($"New AccessToken : {newToken.AccessToken}");
        WriteLine($"New RefreshToken: {newToken.RefreshToken}");
        return newToken;
    }

    private static async Task CallProtectedApiAsync(string requestUrl, Token token)
    {
        WriteLine("\nCall ProtectedApi:");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var response = await _httpClient.GetAsync(requestUrl);
        var message = await response.Content.ReadAsStringAsync();
        WriteLine("URL response: " + message);
    }
}