using ASPNETCore2JwtAuthentication.DomainClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Route("api/[controller]"), EnableCors("CorsPolicy")]
public class ApiSettingsController : Controller
{
    private readonly IOptionsSnapshot<ApiSettings> _apiSettingsConfig;

    public ApiSettingsController(IOptionsSnapshot<ApiSettings> apiSettingsConfig)
    {
        _apiSettingsConfig = apiSettingsConfig ?? throw new ArgumentNullException(nameof(apiSettingsConfig));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Get()
    {
        return Ok(_apiSettingsConfig.Value); // For the Angular Client
    }
}