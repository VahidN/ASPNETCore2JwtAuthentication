using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ASPNETCore2JwtAuthentication.Services;

public interface IDeviceDetectionService
{
    string GetDeviceDetails(HttpContext? context);
    string GetCurrentRequestDeviceDetails();

    string GetDeviceDetailsHash(HttpContext? context);
    string GetCurrentRequestDeviceDetailsHash();

    string? GetUserTokenDeviceDetailsHash(ClaimsIdentity? claimsIdentity);
    string? GetCurrentUserTokenDeviceDetailsHash();

    bool HasUserTokenValidDeviceDetails(ClaimsIdentity? claimsIdentity);
    bool HasCurrentUserTokenValidDeviceDetails();
}