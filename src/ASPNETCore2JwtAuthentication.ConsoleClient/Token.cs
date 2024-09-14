using Newtonsoft.Json;

namespace ASPNETCore2JwtAuthentication.ConsoleClient;

public class Token
{
    [JsonProperty(propertyName: "access_token")]
    public string? AccessToken { get; set; }

    [JsonProperty(propertyName: "refresh_token")]
    public string? RefreshToken { get; set; }
}