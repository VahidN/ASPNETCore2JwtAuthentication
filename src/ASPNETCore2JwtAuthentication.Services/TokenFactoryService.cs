using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ASPNETCore2JwtAuthentication.DomainClasses;
using ASPNETCore2JwtAuthentication.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ASPNETCore2JwtAuthentication.Services;

public class TokenFactoryService : ITokenFactoryService
{
    private readonly IOptionsSnapshot<BearerTokensOptions> _configuration;
    private readonly IDeviceDetectionService _deviceDetectionService;
    private readonly ILogger<TokenFactoryService> _logger;
    private readonly IRolesService _rolesService;
    private readonly ISecurityService _securityService;

    public TokenFactoryService(
        ISecurityService securityService,
        IRolesService rolesService,
        IOptionsSnapshot<BearerTokensOptions> configuration,
        ILogger<TokenFactoryService> logger,
        IDeviceDetectionService deviceDetectionService)
    {
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deviceDetectionService =
            deviceDetectionService ?? throw new ArgumentNullException(nameof(deviceDetectionService));
    }

    public async Task<JwtTokensData> CreateJwtTokensAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var (accessToken, claims) = await createAccessTokenAsync(user);
        var (refreshTokenValue, refreshTokenSerial) = createRefreshToken();
        return new JwtTokensData
               {
                   AccessToken = accessToken,
                   RefreshToken = refreshTokenValue,
                   RefreshTokenSerial = refreshTokenSerial,
                   Claims = claims,
               };
    }

    public string GetRefreshTokenSerial(string refreshTokenValue)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            return null;
        }

        ClaimsPrincipal decodedRefreshTokenPrincipal = null;
        try
        {
            decodedRefreshTokenPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                 refreshTokenValue,
                 new TokenValidationParameters
                 {
                     ValidIssuer = _configuration.Value.Issuer, // site that makes the token
                     ValidAudience = _configuration.Value.Audience, // site that consumes the token
                     RequireExpirationTime = true,
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key)),
                     ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                     ValidateLifetime = true, // validate the expiration
                     ClockSkew = TimeSpan.Zero, // tolerance for the expiration date
                 },
                 out _
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to validate refreshTokenValue: `{refreshTokenValue}`.");
        }

        return decodedRefreshTokenPrincipal?.Claims
                                           ?.FirstOrDefault(c => string.Equals(c.Type, ClaimTypes.SerialNumber,
                                                                               StringComparison.Ordinal))?.Value;
    }

    private (string RefreshTokenValue, string RefreshTokenSerial) createRefreshToken()
    {
        var refreshTokenSerial = _securityService.CreateCryptographicallySecureGuid().ToString()
                                                 .Replace("-", "", StringComparison.Ordinal);

        var claims = new List<Claim>
                     {
                         // Unique Id for all Jwt tokes
                         new(JwtRegisteredClaimNames.Jti,
                             _securityService.CreateCryptographicallySecureGuid().ToString(),
                             ClaimValueTypes.String, _configuration.Value.Issuer),
                         // Issuer
                         new(JwtRegisteredClaimNames.Iss, _configuration.Value.Issuer, ClaimValueTypes.String,
                             _configuration.Value.Issuer),
                         // Issued at
                         new(JwtRegisteredClaimNames.Iat,
                             DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                             ClaimValueTypes.Integer64, _configuration.Value.Issuer),
                         // for invalidation
                         new(ClaimTypes.SerialNumber, refreshTokenSerial, ClaimValueTypes.String,
                             _configuration.Value.Issuer),
                         new(ClaimTypes.System, _deviceDetectionService.GetCurrentRequestDeviceDetailsHash(),
                             ClaimValueTypes.String, _configuration.Value.Issuer),
                     };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
                                         _configuration.Value.Issuer,
                                         _configuration.Value.Audience,
                                         claims,
                                         now,
                                         now.AddMinutes(_configuration.Value.RefreshTokenExpirationMinutes),
                                         creds);
        var refreshTokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return (refreshTokenValue, refreshTokenSerial);
    }

    private async Task<(string AccessToken, IEnumerable<Claim> Claims)> createAccessTokenAsync(User user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var claims = new List<Claim>
                     {
                         // Unique Id for all Jwt tokes
                         new(JwtRegisteredClaimNames.Jti,
                             _securityService.CreateCryptographicallySecureGuid().ToString(),
                             ClaimValueTypes.String, _configuration.Value.Issuer),
                         // Issuer
                         new(JwtRegisteredClaimNames.Iss, _configuration.Value.Issuer, ClaimValueTypes.String,
                             _configuration.Value.Issuer),
                         // Issued at
                         new(JwtRegisteredClaimNames.Iat,
                             DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                             ClaimValueTypes.Integer64, _configuration.Value.Issuer),
                         new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture),
                             ClaimValueTypes.String, _configuration.Value.Issuer),
                         new(ClaimTypes.Name, user.Username, ClaimValueTypes.String, _configuration.Value.Issuer),
                         new("DisplayName", user.DisplayName, ClaimValueTypes.String, _configuration.Value.Issuer),
                         // to invalidate the cookie
                         new(ClaimTypes.SerialNumber, user.SerialNumber, ClaimValueTypes.String,
                             _configuration.Value.Issuer),
                         // custom data
                         new(ClaimTypes.UserData, user.Id.ToString(CultureInfo.InvariantCulture),
                             ClaimValueTypes.String, _configuration.Value.Issuer),
                         new(ClaimTypes.System, _deviceDetectionService.GetCurrentRequestDeviceDetailsHash(),
                             ClaimValueTypes.String, _configuration.Value.Issuer),
                     };

        // add roles
        var roles = await _rolesService.FindUserRolesAsync(user.Id);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, _configuration.Value.Issuer));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
                                         _configuration.Value.Issuer,
                                         _configuration.Value.Audience,
                                         claims,
                                         now,
                                         now.AddMinutes(_configuration.Value.AccessTokenExpirationMinutes),
                                         creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), claims);
    }
}