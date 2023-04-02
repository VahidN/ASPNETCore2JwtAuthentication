using ASPNETCore2JwtAuthentication.DomainClasses;
using ASPNETCore2JwtAuthentication.Models;

namespace ASPNETCore2JwtAuthentication.Services;

public interface ITokenFactoryService
{
    Task<JwtTokensData> CreateJwtTokensAsync(User user);
    string? GetRefreshTokenSerial(string refreshTokenValue);
}