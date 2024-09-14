using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

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
        ArgumentNullException.ThrowIfNull(context);

        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

        if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
        {
            context.Fail(failureMessage: "This is not our issued token. It has no claims.");

            return;
        }

        if (!_deviceDetectionService.HasUserTokenValidDeviceDetails(claimsIdentity))
        {
            context.Fail(failureMessage: "Detected usage of an old token from a new device! Please login again!");

            return;
        }

        var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);

        if (serialNumberClaim == null)
        {
            context.Fail(failureMessage: "This is not our issued token. It has no serial.");

            return;
        }

        var userIdString = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

        if (!int.TryParse(userIdString, NumberStyles.Number, CultureInfo.InvariantCulture, out var userId))
        {
            context.Fail(failureMessage: "This is not our issued token. It has no user-id.");

            return;
        }

        var user = await _usersService.FindUserAsync(userId);

        if (user == null || !string.Equals(user.SerialNumber, serialNumberClaim.Value, StringComparison.Ordinal) ||
            !user.IsActive)
        {
            // user has changed his/her password/roles/stat/IsActive
            context.Fail(failureMessage: "This token is expired. Please login again.");
        }

        if (context.SecurityToken is not JsonWebToken accessToken ||
            string.IsNullOrWhiteSpace(accessToken.UnsafeToString()) ||
            !await _tokenStoreService.IsValidTokenAsync(accessToken.UnsafeToString(), userId))
        {
            context.Fail(failureMessage: "This token is not in our database.");

            return;
        }

        await _usersService.UpdateUserLastActivityDateAsync(userId);
    }
}