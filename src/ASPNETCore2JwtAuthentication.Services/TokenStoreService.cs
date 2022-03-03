using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.Services;

public class TokenStoreService : ITokenStoreService
{
    private readonly IOptionsSnapshot<BearerTokensOptions> _configuration;
    private readonly ISecurityService _securityService;
    private readonly ITokenFactoryService _tokenFactoryService;
    private readonly DbSet<UserToken> _tokens;

    public TokenStoreService(
        IUnitOfWork uow,
        ISecurityService securityService,
        IOptionsSnapshot<BearerTokensOptions> configuration,
        ITokenFactoryService tokenFactoryService)
    {
        if (uow is null)
        {
            throw new ArgumentNullException(nameof(uow));
        }

        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _tokens = uow.Set<UserToken>();
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _tokenFactoryService = tokenFactoryService ?? throw new ArgumentNullException(nameof(tokenFactoryService));
    }

    public async Task AddUserTokenAsync(UserToken userToken)
    {
        if (userToken == null)
        {
            throw new ArgumentNullException(nameof(userToken));
        }

        if (!_configuration.Value.AllowMultipleLoginsFromTheSameUser)
        {
            await InvalidateUserTokensAsync(userToken.UserId);
        }

        await DeleteTokensWithSameRefreshTokenSourceAsync(userToken.RefreshTokenIdHashSource);
        _tokens.Add(userToken);
    }

    public async Task AddUserTokenAsync(User user, string refreshTokenSerial, string accessToken,
        string refreshTokenSourceSerial)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var now = DateTimeOffset.UtcNow;
        var token = new UserToken
        {
            UserId = user.Id,
            // Refresh token handles should be treated as secrets and should be stored hashed
            RefreshTokenIdHash = _securityService.GetSha256Hash(refreshTokenSerial),
            RefreshTokenIdHashSource = string.IsNullOrWhiteSpace(refreshTokenSourceSerial)
                ? null
                : _securityService.GetSha256Hash(refreshTokenSourceSerial),
            AccessTokenHash = _securityService.GetSha256Hash(accessToken),
            RefreshTokenExpiresDateTime = now.AddMinutes(_configuration.Value.RefreshTokenExpirationMinutes),
            AccessTokenExpiresDateTime = now.AddMinutes(_configuration.Value.AccessTokenExpirationMinutes)
        };
        await AddUserTokenAsync(token);
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var now = DateTimeOffset.UtcNow;
        await _tokens.Where(x => x.RefreshTokenExpiresDateTime < now)
            .ForEachAsync(userToken => _tokens.Remove(userToken));
    }

    public async Task DeleteTokenAsync(string refreshTokenValue)
    {
        var token = await FindTokenAsync(refreshTokenValue);
        if (token != null)
        {
            _tokens.Remove(token);
        }
    }

    public async Task DeleteTokensWithSameRefreshTokenSourceAsync(string refreshTokenIdHashSource)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenIdHashSource))
        {
            return;
        }

        await _tokens.Where(t => t.RefreshTokenIdHashSource == refreshTokenIdHashSource ||
                                 t.RefreshTokenIdHash == refreshTokenIdHashSource &&
                                 t.RefreshTokenIdHashSource == null)
            .ForEachAsync(userToken => _tokens.Remove(userToken));
    }

    public async Task RevokeUserBearerTokensAsync(string userIdValue, string refreshTokenValue)
    {
        if (!string.IsNullOrWhiteSpace(userIdValue) &&
            int.TryParse(userIdValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var userId))
        {
            if (_configuration.Value.AllowSignoutAllUserActiveClients)
            {
                await InvalidateUserTokensAsync(userId);
            }
        }

        if (!string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            var refreshTokenSerial = _tokenFactoryService.GetRefreshTokenSerial(refreshTokenValue);
            if (!string.IsNullOrWhiteSpace(refreshTokenSerial))
            {
                var refreshTokenIdHashSource = _securityService.GetSha256Hash(refreshTokenSerial);
                await DeleteTokensWithSameRefreshTokenSourceAsync(refreshTokenIdHashSource);
            }
        }

        await DeleteExpiredTokensAsync();
    }

    public Task<UserToken> FindTokenAsync(string refreshTokenValue)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            return Task.FromResult<UserToken>(null);
        }

        var refreshTokenSerial = _tokenFactoryService.GetRefreshTokenSerial(refreshTokenValue);
        if (string.IsNullOrWhiteSpace(refreshTokenSerial))
        {
            return Task.FromResult<UserToken>(null);
        }

        var refreshTokenIdHash = _securityService.GetSha256Hash(refreshTokenSerial);
        return _tokens.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshTokenIdHash == refreshTokenIdHash);
    }

    public async Task InvalidateUserTokensAsync(int userId)
    {
        await _tokens.Where(x => x.UserId == userId)
            .ForEachAsync(userToken => _tokens.Remove(userToken));
    }

    public async Task<bool> IsValidTokenAsync(string accessToken, int userId)
    {
        var accessTokenHash = _securityService.GetSha256Hash(accessToken);
        var userToken = await _tokens.FirstOrDefaultAsync(
            x => x.AccessTokenHash == accessTokenHash && x.UserId == userId);
        return userToken?.AccessTokenExpiresDateTime >= DateTimeOffset.UtcNow;
    }
}