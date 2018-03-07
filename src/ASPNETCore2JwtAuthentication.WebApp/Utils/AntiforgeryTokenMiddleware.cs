using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ASPNETCore2JwtAuthentication.WebApp.Utils
{
    public class AngularAntiforgeryTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        public AngularAntiforgeryTokenMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (path != null && !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                var tokens = _antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append(
                      key: "XSRF-TOKEN",
                      value: tokens.RequestToken,
                      options: new CookieOptions
                      {
                          HttpOnly = false // Now JavaScript is able to read the cookie
                      });
            }
            return _next(context);
        }
    }

    public static class AntiforgeryTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseAngularAntiforgeryToken(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AngularAntiforgeryTokenMiddleware>();
        }
    }
}