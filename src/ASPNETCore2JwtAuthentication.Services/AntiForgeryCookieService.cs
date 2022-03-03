using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.Services;

public class AntiForgeryCookieService : IAntiForgeryCookieService
{
    private const string XsrfTokenKey = "XSRF-TOKEN";
    private readonly IAntiforgery _antiforgery;
    private readonly IOptions<AntiforgeryOptions> _antiforgeryOptions;

    private readonly IHttpContextAccessor _contextAccessor;

    public AntiForgeryCookieService(
        IHttpContextAccessor contextAccessor,
        IAntiforgery antiforgery,
        IOptions<AntiforgeryOptions> antiforgeryOptions)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
        _antiforgeryOptions = antiforgeryOptions ?? throw new ArgumentNullException(nameof(antiforgeryOptions));
    }

    public void RegenerateAntiForgeryCookies(IEnumerable<Claim> claims)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext is null)
        {
            throw new InvalidOperationException("httpContext is null");
        }

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
        var tokens = _antiforgery.GetAndStoreTokens(httpContext);
        if (tokens.RequestToken is null)
        {
            throw new InvalidOperationException("tokens.RequestToken is null");
        }

        httpContext.Response.Cookies.Append(
            XsrfTokenKey,
            tokens.RequestToken,
            new CookieOptions
            {
                HttpOnly = false // Now JavaScript is able to read the cookie
            });
    }

    public void DeleteAntiForgeryCookies()
    {
        var cookies = _contextAccessor.HttpContext?.Response.Cookies;
        if (cookies is null)
        {
            return;
        }

        var cookieName = _antiforgeryOptions.Value.Cookie.Name;
        if (string.IsNullOrWhiteSpace(cookieName))
        {
            return;
        }

        cookies.Delete(cookieName);
        cookies.Delete(XsrfTokenKey);
    }
}