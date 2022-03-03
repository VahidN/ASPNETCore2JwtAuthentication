using ASPNETCore2JwtAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore2JwtAuthentication.WebApp.Controllers;

[Route("api/[controller]"), EnableCors("CorsPolicy"), Authorize(Policy = CustomRoles.Editor)]
public class MyProtectedEditorsApiController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Id = 1,
            Title = "Hello from My Protected Editors Controller! [Authorize(Policy = CustomRoles.Editor)]",
            Username = User.Identity?.Name
        });
    }
}