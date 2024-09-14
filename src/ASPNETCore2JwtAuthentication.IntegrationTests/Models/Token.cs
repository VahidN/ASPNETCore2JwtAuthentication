namespace ASPNETCore2JwtAuthentication.IntegrationTests.Models;

public class Token
{
    [JsonPropertyName(name: "access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName(name: "refresh_token")]
    public string? RefreshToken { get; set; }
}