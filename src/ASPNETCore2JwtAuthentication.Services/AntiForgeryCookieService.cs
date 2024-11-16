using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.Services;

public class AntiForgeryCookieService(
    IHttpContextAccessor contextAccessor,
    IAntiforgery antiforgery,
    IOptions<AntiforgeryOptions> antiforgeryOptions) : IAntiForgeryCookieService
{
    private const string XsrfTokenKey = "XSRF-TOKEN";
    private readonly IAntiforgery _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));

    private readonly IOptions<AntiforgeryOptions> _antiforgeryOptions =
        antiforgeryOptions ?? throw new ArgumentNullException(nameof(antiforgeryOptions));

    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

    public void RegenerateAntiForgeryCookies(IEnumerable<Claim> claims)
    {
        var httpContext = _contextAccessor.HttpContext ??
                          throw new InvalidOperationException(message: "httpContext is null");

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
        var tokens = _antiforgery.GetAndStoreTokens(httpContext);

        if (tokens.RequestToken is null)
        {
            throw new InvalidOperationException(message: "tokens.RequestToken is null");
        }

        httpContext.Response.Cookies.Append(XsrfTokenKey, tokens.RequestToken, new CookieOptions
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