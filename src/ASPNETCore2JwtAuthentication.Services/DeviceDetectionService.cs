using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using UAParser;

namespace ASPNETCore2JwtAuthentication.Services;

/// <summary>
///     To invalidate an old user's token from a new device
/// </summary>
public class DeviceDetectionService : IDeviceDetectionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecurityService _securityService;

    public DeviceDetectionService(ISecurityService securityService, IHttpContextAccessor httpContextAccessor)
    {
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GetCurrentRequestDeviceDetails() => GetDeviceDetails(_httpContextAccessor.HttpContext);

    public string GetDeviceDetails(HttpContext context)
    {
        var ua = GetUserAgent(context);
        if (ua is null)
        {
            return "unknown";
        }

        var client = Parser.GetDefault().Parse(ua);
        var deviceInfo = client.Device.Family;
        var browserInfo = $"{client.UA.Family}, {client.UA.Major}.{client.UA.Minor}";
        var osInfo = $"{client.OS.Family}, {client.OS.Major}.{client.OS.Minor}";
        //TODO: Add the user's IP address here, if it's a banking system.
        return $"{deviceInfo}, {browserInfo}, {osInfo}";
    }

    public string GetDeviceDetailsHash(HttpContext context) =>
        _securityService.GetSha256Hash(GetDeviceDetails(context));

    public string GetCurrentRequestDeviceDetailsHash() => GetDeviceDetailsHash(_httpContextAccessor.HttpContext);

    public string GetCurrentUserTokenDeviceDetailsHash() =>
        GetUserTokenDeviceDetailsHash(_httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity);

    public string GetUserTokenDeviceDetailsHash(ClaimsIdentity claimsIdentity)
    {
        if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
        {
            return null;
        }

        return claimsIdentity.FindFirst(ClaimTypes.System)?.Value;
    }

    public bool HasCurrentUserTokenValidDeviceDetails() =>
        HasUserTokenValidDeviceDetails(_httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity);

    public bool HasUserTokenValidDeviceDetails(ClaimsIdentity claimsIdentity) =>
        string.Equals(GetCurrentRequestDeviceDetailsHash(), GetUserTokenDeviceDetailsHash(claimsIdentity),
                      StringComparison.Ordinal);

    private static string GetUserAgent(HttpContext context)
    {
        if (context is null)
        {
            return null;
        }

        return context.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent)
                   ? userAgent.ToString()
                   : null;
    }
}