using ASPNETCore2JwtAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Route(template: "api/[controller]")]
[EnableCors(policyName: "CorsPolicy")]
public class ApiSettingsController(IOptionsSnapshot<ApiSettings> apiSettingsConfig) : Controller
{
    private readonly IOptionsSnapshot<ApiSettings> _apiSettingsConfig =
        apiSettingsConfig ?? throw new ArgumentNullException(nameof(apiSettingsConfig));

    [AllowAnonymous] [HttpGet] public IActionResult Get() => Ok(_apiSettingsConfig.Value); // For the Angular Client
}