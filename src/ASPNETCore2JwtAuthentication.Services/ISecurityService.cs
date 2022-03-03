namespace ASPNETCore2JwtAuthentication.Services;

public interface ISecurityService
{
    string GetSha256Hash(string input);
    Guid CreateCryptographicallySecureGuid();
}