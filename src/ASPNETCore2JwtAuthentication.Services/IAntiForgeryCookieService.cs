using System.Security.Claims;

namespace ASPNETCore2JwtAuthentication.Services;

public interface IAntiForgeryCookieService
{
    void RegenerateAntiForgeryCookies(IEnumerable<Claim> claims);
    void DeleteAntiForgeryCookies();
}