namespace ASPNETCore2JwtAuthentication.Models;

public class AdminUserSeed
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? DisplayName { get; set; }
}