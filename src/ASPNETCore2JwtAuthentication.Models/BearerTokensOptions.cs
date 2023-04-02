namespace ASPNETCore2JwtAuthentication.Models;

public class BearerTokensOptions
{
    public required string Key { set; get; }
    public required string Issuer { set; get; }
    public required string Audience { set; get; }
    public int AccessTokenExpirationMinutes { set; get; }
    public int RefreshTokenExpirationMinutes { set; get; }
    public bool AllowMultipleLoginsFromTheSameUser { set; get; }
    public bool AllowSignoutAllUserActiveClients { set; get; }
}