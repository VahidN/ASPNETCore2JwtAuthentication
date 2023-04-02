namespace ASPNETCore2JwtAuthentication.Models;

public class Token
{
    [JsonPropertyName("refreshToken")]
    [Required]
    public required string RefreshToken { get; set; }
}