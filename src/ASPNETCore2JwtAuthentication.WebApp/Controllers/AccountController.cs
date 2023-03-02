using System.Security.Claims;
using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using ASPNETCore2JwtAuthentication.Services;
using ASPNETCore2JwtAuthentication.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Route("api/[controller]"), EnableCors("CorsPolicy")]
public class AccountController : Controller
{
    private readonly IAntiForgeryCookieService _antiforgery;
    private readonly ITokenFactoryService _tokenFactoryService;
    private readonly ITokenStoreService _tokenStoreService;
    private readonly IUnitOfWork _uow;
    private readonly IUsersService _usersService;

    public AccountController(
        IUsersService usersService,
        ITokenStoreService tokenStoreService,
        ITokenFactoryService tokenFactoryService,
        IUnitOfWork uow,
        IAntiForgeryCookieService antiforgery)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        _tokenStoreService = tokenStoreService ?? throw new ArgumentNullException(nameof(tokenStoreService));
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
        _tokenFactoryService = tokenFactoryService ?? throw new ArgumentNullException(nameof(tokenFactoryService));
    }

    [AllowAnonymous, IgnoreAntiforgeryToken, HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] User loginUser)
    {
        if (loginUser == null)
        {
            return BadRequest("user is not set.");
        }

        var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password);
        if (user?.IsActive != true)
        {
            return Unauthorized();
        }

        var result = await _tokenFactoryService.CreateJwtTokensAsync(user);
        await _tokenStoreService.AddUserTokenAsync(user, result.RefreshTokenSerial, result.AccessToken, null);
        await _uow.SaveChangesAsync();

        _antiforgery.RegenerateAntiForgeryCookies(result.Claims);

        return Ok(new { access_token = result.AccessToken, refresh_token = result.RefreshToken });
    }

    [AllowAnonymous, IgnoreAntiforgeryToken, HttpPost("[action]")]
    public async Task<IActionResult> RefreshToken([FromBody] Token model)
    {
        if (model == null)
        {
            return BadRequest();
        }

        var refreshTokenValue = model.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            return BadRequest("refreshToken is not set.");
        }

        var token = await _tokenStoreService.FindTokenAsync(refreshTokenValue);
        if (token == null)
        {
            return Unauthorized("This is not our token!");
        }

        var result = await _tokenFactoryService.CreateJwtTokensAsync(token.User);
        await _tokenStoreService.AddUserTokenAsync(token.User, result.RefreshTokenSerial, result.AccessToken,
            _tokenFactoryService.GetRefreshTokenSerial(refreshTokenValue));
        await _uow.SaveChangesAsync();

        _antiforgery.RegenerateAntiForgeryCookies(result.Claims);

        return Ok(new { access_token = result.AccessToken, refresh_token = result.RefreshToken });
    }

    [AllowAnonymous, HttpGet("[action]")]
    public async Task<bool> Logout(string refreshToken)
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var userIdValue = claimsIdentity?.FindFirst(ClaimTypes.UserData)?.Value;

        // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
        // Delete the user's tokens from the database (revoke its bearer token)
        await _tokenStoreService.RevokeUserBearerTokensAsync(userIdValue, refreshToken);
        await _uow.SaveChangesAsync();

        _antiforgery.DeleteAntiForgeryCookies();

        return true;
    }

    [HttpGet("[action]"), HttpPost("[action]")]
    public bool IsAuthenticated()
    {
        return User.Identity?.IsAuthenticated ?? false;
    }

    [HttpGet("[action]"), HttpPost("[action]")]
    public IActionResult GetUserInfo()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        return Json(new { Username = claimsIdentity?.Name });
    }
}

//TODO: Delete old tokens using a background job