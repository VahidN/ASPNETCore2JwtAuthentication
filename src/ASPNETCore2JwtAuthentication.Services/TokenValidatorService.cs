using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ASPNETCore2JwtAuthentication.Services;

public class TokenValidatorService : ITokenValidatorService
{
    private readonly IDeviceDetectionService _deviceDetectionService;
    private readonly ITokenStoreService _tokenStoreService;
    private readonly IUsersService _usersService;

    public TokenValidatorService(IUsersService usersService,
                                 ITokenStoreService tokenStoreService,
                                 IDeviceDetectionService deviceDetectionService)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        _tokenStoreService = tokenStoreService ?? throw new ArgumentNullException(nameof(tokenStoreService));
        _deviceDetectionService =
            deviceDetectionService ?? throw new ArgumentNullException(nameof(deviceDetectionService));
    }

    public async Task ValidateAsync(TokenValidatedContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
        if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
        {
            context.Fail("This is not our issued token. It has no claims.");
            return;
        }

        if (!_deviceDetectionService.HasUserTokenValidDeviceDetails(claimsIdentity))
        {
            context.Fail("Detected usage of an old token from a new device! Please login again!");
            return;
        }

        var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);
        if (serialNumberClaim == null)
        {
            context.Fail("This is not our issued token. It has no serial.");
            return;
        }

        var userIdString = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
        if (!int.TryParse(userIdString, NumberStyles.Number, CultureInfo.InvariantCulture, out var userId))
        {
            context.Fail("This is not our issued token. It has no user-id.");
            return;
        }

        var user = await _usersService.FindUserAsync(userId);
        if (user == null || !string.Equals(user.SerialNumber, serialNumberClaim.Value, StringComparison.Ordinal) ||
            !user.IsActive)
        {
            // user has changed his/her password/roles/stat/IsActive
            context.Fail("This token is expired. Please login again.");
        }

        if (!(context.SecurityToken is JwtSecurityToken accessToken) ||
            string.IsNullOrWhiteSpace(accessToken.RawData) ||
            !await _tokenStoreService.IsValidTokenAsync(accessToken.RawData, userId))
        {
            context.Fail("This token is not in our database.");
            return;
        }

        await _usersService.UpdateUserLastActivityDateAsync(userId);
    }
}