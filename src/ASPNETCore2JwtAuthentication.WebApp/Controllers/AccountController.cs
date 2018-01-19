using System.Security.Claims;
using System.Threading.Tasks;
using ASPNETCore2JwtAuthentication.Common;
using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using ASPNETCore2JwtAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class AccountController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly IUnitOfWork _uow;
        private readonly IOptionsSnapshot<BearerTokensOptions> _configuration;

        public AccountController(
            IUsersService usersService,
            ITokenStoreService tokenStoreService,
            IUnitOfWork uow,
            IOptionsSnapshot<BearerTokensOptions> configuration)
        {
            _usersService = usersService;
            _usersService.CheckArgumentIsNull(nameof(usersService));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(_tokenStoreService));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _configuration = configuration;
            _configuration.CheckArgumentIsNull(nameof(configuration));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]  User loginUser)
        {
            if (loginUser == null)
            {
                return BadRequest("user is not set.");
            }

            var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password);
            if (user == null || !user.IsActive)
            {
                return Unauthorized();
            }

            var (accessToken, refreshToken) = await _tokenStoreService.CreateJwtTokens(user);
            return Ok(new { access_token = accessToken, refresh_token = refreshToken });
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody]JToken jsonBody)
        {
            var refreshToken = jsonBody.Value<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("refreshToken is not set.");
            }

            var token = await _tokenStoreService.FindTokenAsync(refreshToken);
            if (token == null)
            {
                return Unauthorized();
            }

            var (accessToken, newRefreshToken) = await _tokenStoreService.CreateJwtTokens(token.User);
            return Ok(new { access_token = accessToken, refresh_token = newRefreshToken });
        }

        [AllowAnonymous]
        [HttpGet("[action]"), HttpPost("[action]")]
        public async Task<bool> Logout()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

            // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
            // Delete the user's tokens from the database (revoke its bearer token)
            if (!string.IsNullOrWhiteSpace(userIdValue) && int.TryParse(userIdValue, out int userId))
            {
                if (_configuration.Value.AllowSignoutAllUserActiveClients)
                {
                    await _tokenStoreService.InvalidateUserTokensAsync(userId);
                }
            }
            await _tokenStoreService.DeleteExpiredTokensAsync();
            await _uow.SaveChangesAsync();

            return true;
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public bool IsAuthenthenticated()
        {
            return User.Identity.IsAuthenticated;
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public IActionResult GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return Json(new { Username = claimsIdentity.Name });
        }
    }
}