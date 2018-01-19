using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ASPNETCore2JwtAuthentication.Common;
using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ASPNETCore2JwtAuthentication.Services
{
    public interface ITokenStoreService
    {
        Task AddUserTokenAsync(UserToken userToken);
        Task AddUserTokenAsync(
                User user, string refreshToken, string accessToken,
                DateTimeOffset refreshTokenExpiresDateTime, DateTimeOffset accessTokenExpiresDateTime);
        Task<bool> IsValidTokenAsync(string accessToken, int userId);
        Task DeleteExpiredTokensAsync();
        Task<UserToken> FindTokenAsync(string refreshToken);
        Task DeleteTokenAsync(string refreshToken);
        Task InvalidateUserTokensAsync(int userId);
        Task<(string accessToken, string refreshToken)> CreateJwtTokens(User user);
    }

    public class TokenStoreService : ITokenStoreService
    {
        private readonly ISecurityService _securityService;
        private readonly IUnitOfWork _uow;
        private readonly DbSet<UserToken> _tokens;
        private readonly IOptionsSnapshot<BearerTokensOptions> _configuration;
        private readonly IRolesService _rolesService;

        public TokenStoreService(
            IUnitOfWork uow,
            ISecurityService securityService,
            IRolesService rolesService,
            IOptionsSnapshot<BearerTokensOptions> configuration)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _securityService = securityService;
            _securityService.CheckArgumentIsNull(nameof(_securityService));

            _rolesService = rolesService;
            _rolesService.CheckArgumentIsNull(nameof(rolesService));

            _tokens = _uow.Set<UserToken>();

            _configuration = configuration;
            _configuration.CheckArgumentIsNull(nameof(configuration));
        }

        public async Task AddUserTokenAsync(UserToken userToken)
        {
            if (!_configuration.Value.AllowMultipleLoginsFromTheSameUser)
            {
                await InvalidateUserTokensAsync(userToken.UserId);
            }
            _tokens.Add(userToken);
        }

        public async Task AddUserTokenAsync(
                User user, string refreshToken, string accessToken,
                DateTimeOffset refreshTokenExpiresDateTime, DateTimeOffset accessTokenExpiresDateTime)
        {
            var token = new UserToken
            {
                UserId = user.Id,
                // Refresh token handles should be treated as secrets and should be stored hashed
                RefreshTokenIdHash = _securityService.GetSha256Hash(refreshToken),
                AccessTokenHash = _securityService.GetSha256Hash(accessToken),
                RefreshTokenExpiresDateTime = refreshTokenExpiresDateTime,
                AccessTokenExpiresDateTime = accessTokenExpiresDateTime
            };
            await AddUserTokenAsync(token);
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var userTokens = await _tokens.Where(x => x.RefreshTokenExpiresDateTime < now).ToListAsync();
            foreach (var userToken in userTokens)
            {
                _tokens.Remove(userToken);
            }
        }

        public async Task DeleteTokenAsync(string refreshToken)
        {
            var token = await FindTokenAsync(refreshToken);
            if (token != null)
            {
                _tokens.Remove(token);
            }
        }

        public Task<UserToken> FindTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }
            var refreshTokenIdHash = _securityService.GetSha256Hash(refreshToken);
            return _tokens.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshTokenIdHash == refreshTokenIdHash);
        }

        public async Task InvalidateUserTokensAsync(int userId)
        {
            var userTokens = await _tokens.Where(x => x.UserId == userId).ToListAsync();
            foreach (var userToken in userTokens)
            {
                _tokens.Remove(userToken);
            }
        }

        public async Task<bool> IsValidTokenAsync(string accessToken, int userId)
        {
            var accessTokenHash = _securityService.GetSha256Hash(accessToken);
            var userToken = await _tokens.FirstOrDefaultAsync(
                x => x.AccessTokenHash == accessTokenHash && x.UserId == userId);
            return userToken?.AccessTokenExpiresDateTime >= DateTime.UtcNow;
        }

        public async Task<(string accessToken, string refreshToken)> CreateJwtTokens(User user)
        {
            var now = DateTimeOffset.UtcNow;
            var accessTokenExpiresDateTime = now.AddMinutes(_configuration.Value.AccessTokenExpirationMinutes);
            var refreshTokenExpiresDateTime = now.AddMinutes(_configuration.Value.RefreshTokenExpirationMinutes);
            var accessToken = await createAccessTokenAsync(user, accessTokenExpiresDateTime.UtcDateTime);
            var refreshToken = Guid.NewGuid().ToString().Replace("-", "");

            await AddUserTokenAsync(user, refreshToken, accessToken, refreshTokenExpiresDateTime, accessTokenExpiresDateTime);
            await _uow.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        private async Task<string> createAccessTokenAsync(User user, DateTime expires)
        {
            var claims = new List<Claim>
            {
                // Unique Id for all Jwt tokes
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Issuer
                new Claim(JwtRegisteredClaimNames.Iss, _configuration.Value.Issuer),
                // Issued at
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("DisplayName", user.DisplayName),
                // to invalidate the cookie
                new Claim(ClaimTypes.SerialNumber, user.SerialNumber),
                // custom data
                new Claim(ClaimTypes.UserData, user.Id.ToString())
            };

            // add roles
            var roles = await _rolesService.FindUserRolesAsync(user.Id);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration.Value.Issuer,
                audience: _configuration.Value.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}