using Newtonsoft.Json;

namespace ASPNETCore2JwtAuthentication.ConsoleClient;

public class Token
{
    [JsonProperty("access_token")] public string AccessToken { get; set; }

    [JsonProperty("refresh_token")] public string RefreshToken { get; set; }
}