using System.Collections.Generic;
using System.Security.Claims;
using ASPNETCore2JwtAuthentication.Common;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.Services
{
    public interface IAntiForgeryCookieService
    {
        void RegenerateAntiForgeryCookies(IEnumerable<Claim> claims);
        void DeleteAntiForgeryCookies();
    }

    public class AntiForgeryCookieService : IAntiForgeryCookieService
    {
        private const string XsrfTokenKey = "XSRF-TOKEN";

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAntiforgery _antiforgery;
        private readonly IOptions<AntiforgeryOptions> _antiforgeryOptions;

        public AntiForgeryCookieService(
            IHttpContextAccessor contextAccessor,
            IAntiforgery antiforgery,
            IOptions<AntiforgeryOptions> antiforgeryOptions)
        {
            _contextAccessor = contextAccessor;
            _contextAccessor.CheckArgumentIsNull(nameof(contextAccessor));

            _antiforgery = antiforgery;
            _antiforgery.CheckArgumentIsNull(nameof(antiforgery));

            _antiforgeryOptions = antiforgeryOptions;
            _antiforgeryOptions.CheckArgumentIsNull(nameof(antiforgeryOptions));
        }

        public void RegenerateAntiForgeryCookies(IEnumerable<Claim> claims)
        {
            var httpContext = _contextAccessor.HttpContext;
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
            var tokens = _antiforgery.GetAndStoreTokens(httpContext);
            httpContext.Response.Cookies.Append(
                  key: XsrfTokenKey,
                  value: tokens.RequestToken,
                  options: new CookieOptions
                  {
                      HttpOnly = false // Now JavaScript is able to read the cookie
                  });
        }

        public void DeleteAntiForgeryCookies()
        {
            var cookies = _contextAccessor.HttpContext.Response.Cookies;
            cookies.Delete(_antiforgeryOptions.Value.Cookie.Name);
            cookies.Delete(XsrfTokenKey);
        }
    }
}