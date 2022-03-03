using System.Text;

namespace ASPNETCore2JwtAuthentication.Services;

public class SecurityService : ISecurityService
{
    private readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();

    public string GetSha256Hash(string input)
    {
        var byteValue = Encoding.UTF8.GetBytes(input);
        var byteHash = SHA256.HashData(byteValue);
        return Convert.ToBase64String(byteHash);
    }

    public Guid CreateCryptographicallySecureGuid()
    {
        var bytes = new byte[16];
        _rand.GetBytes(bytes);
        return new Guid(bytes);
    }
}